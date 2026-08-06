import sqlite3 from 'sqlite3';
import { open } from 'sqlite';
import path from 'path';
import { fileURLToPath } from 'url';
import dotenv from 'dotenv';

dotenv.config({ path: '.env' });

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

let db = null;

export async function initDb() {
  if (db) return db;

  const dbPath = process.env.DATABASE_PATH || path.join(__dirname, 'app.db');

  db = await open({
    filename: dbPath,
    driver: sqlite3.Database
  });

  return db;
}

export function getDb() {
  if (!db) {
    throw new Error('Database not initialized. Call initDb() first.');
  }
  return db;
}

export async function closeDb() {
  if (db) {
    await db.close();
    db = null;
  }
}
