import { readdir, readFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const migrationsDirectory = path.join(path.dirname(fileURLToPath(import.meta.url)), 'migrations');

export async function runMigrations(pool) {
  await pool.query(`
    CREATE TABLE IF NOT EXISTS schema_migrations (
      name TEXT PRIMARY KEY,
      applied_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
    )
  `);

  const migrationFiles = (await readdir(migrationsDirectory))
    .filter((fileName) => /^\d+_.+\.sql$/.test(fileName))
    .sort();

  for (const fileName of migrationFiles) {
    const { rows } = await pool.query(
      'SELECT 1 FROM schema_migrations WHERE name = $1',
      [fileName]
    );

    if (rows.length > 0) {
      continue;
    }

    if (fileName === '001_initial_schema.sql') {
      const existingSchema = await pool.query("SELECT to_regclass('public.users') AS table_name");
      if (existingSchema.rows[0].table_name) {
        await pool.query('INSERT INTO schema_migrations (name) VALUES ($1)', [fileName]);
        continue;
      }
    }

    const migrationSql = await readFile(path.join(migrationsDirectory, fileName), 'utf8');
    const client = await pool.connect();

    try {
      await client.query('BEGIN');
      await client.query(migrationSql);
      await client.query('INSERT INTO schema_migrations (name) VALUES ($1)', [fileName]);
      await client.query('COMMIT');
    } catch (error) {
      await client.query('ROLLBACK');
      throw error;
    } finally {
      client.release();
    }
  }
}
