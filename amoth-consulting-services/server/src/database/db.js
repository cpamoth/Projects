import dotenv from 'dotenv';
import pg from 'pg';

dotenv.config();

const { Pool } = pg;

let pool = null;

const camelCaseColumns = [
  'fullName',
  'createdAt',
  'updatedAt',
  'invoiceNumber',
  'userId',
  'dueDate',
  'completionRate',
  'startDate',
  'endDate',
  'projectId',
  'scheduledDate'
];

const normalizeQuery = (query) => {
  let normalized = query;

  for (const column of camelCaseColumns) {
    normalized = normalized.replace(new RegExp(`\\b${column}\\b`, 'g'), `"${column}"`);
  }

  let parameterIndex = 0;
  normalized = normalized.replace(/\?/g, () => `$${++parameterIndex}`);

  if (/^\s*INSERT\s/i.test(normalized) && !/\bRETURNING\b/i.test(normalized)) {
    normalized += ' RETURNING id';
  }

  return normalized;
};

export async function initDb({ poolFactory = Pool } = {}) {
  if (pool) return pool;

  if (process.env.NODE_ENV === 'production' && process.env.DATABASE_SSL !== 'true') {
    throw new Error('DATABASE_SSL=true is required in production');
  }

  const ssl = process.env.DATABASE_SSL === 'true'
    ? {
        rejectUnauthorized: process.env.DATABASE_SSL_REJECT_UNAUTHORIZED !== 'false'
      }
    : false;

  pool = new poolFactory({
    connectionString: process.env.DATABASE_URL,
    host: process.env.DATABASE_HOST || 'localhost',
    port: Number(process.env.DATABASE_PORT || 5432),
    database: process.env.DATABASE_NAME || 'amoth',
    user: process.env.DATABASE_USERNAME || 'postgres',
    password: process.env.DATABASE_PASSWORD,
    ssl
  });

  return pool;
}

export function getDb() {
  if (!pool) {
    throw new Error('Database not initialized. Call initDb() first.');
  }

  return {
    async get(query, params = []) {
      const result = await pool.query(normalizeQuery(query), params);
      return result.rows[0];
    },
    async all(query, params = []) {
      const result = await pool.query(normalizeQuery(query), params);
      return result.rows;
    },
    async run(query, params = []) {
      const result = await pool.query(normalizeQuery(query), params);
      return {
        lastID: result.rows[0]?.id,
        changes: result.rowCount
      };
    }
  };
}

export async function closeDb() {
  if (pool) {
    await pool.end();
    pool = null;
  }
}
