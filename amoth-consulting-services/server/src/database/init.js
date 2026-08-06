import sqlite3 from 'sqlite3';
import { open } from 'sqlite';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const dbPath = path.join(__dirname, 'app.db');

async function initializeDatabase() {
  try {
    const db = await open({
      filename: dbPath,
      driver: sqlite3.Database
    });

    console.log('Initializing database...');

    // Create users table
    await db.exec(`
      CREATE TABLE IF NOT EXISTS users (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        username TEXT UNIQUE NOT NULL,
        email TEXT UNIQUE NOT NULL,
        password TEXT NOT NULL,
        fullName TEXT,
        bio TEXT,
        createdAt DATETIME DEFAULT CURRENT_TIMESTAMP,
        updatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
      )
    `);

    // Create invoices table
    await db.exec(`
      CREATE TABLE IF NOT EXISTS invoices (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        invoiceNumber TEXT UNIQUE NOT NULL,
        userId INTEGER NOT NULL,
        amount REAL NOT NULL,
        dueDate DATE NOT NULL,
        description TEXT,
        status TEXT DEFAULT 'Pending',
        createdAt DATETIME DEFAULT CURRENT_TIMESTAMP,
        updatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
        FOREIGN KEY (userId) REFERENCES users(id)
      )
    `);

    // Create projects table
    await db.exec(`
      CREATE TABLE IF NOT EXISTS projects (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        name TEXT NOT NULL,
        description TEXT,
        userId INTEGER NOT NULL,
        status TEXT DEFAULT 'Active',
        completionRate REAL DEFAULT 0,
        startDate DATE,
        endDate DATE,
        createdAt DATETIME DEFAULT CURRENT_TIMESTAMP,
        updatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
        FOREIGN KEY (userId) REFERENCES users(id)
      )
    `);

    // Create tasks table
    await db.exec(`
      CREATE TABLE IF NOT EXISTS tasks (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        title TEXT NOT NULL,
        description TEXT,
        projectId INTEGER,
        userId INTEGER NOT NULL,
        status TEXT DEFAULT 'Pending',
        dueDate DATE,
        completed BOOLEAN DEFAULT 0,
        createdAt DATETIME DEFAULT CURRENT_TIMESTAMP,
        updatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
        FOREIGN KEY (projectId) REFERENCES projects(id),
        FOREIGN KEY (userId) REFERENCES users(id)
      )
    `);

    // Create meetings table
    await db.exec(`
      CREATE TABLE IF NOT EXISTS meetings (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        title TEXT NOT NULL,
        description TEXT,
        userId INTEGER NOT NULL,
        scheduledDate DATETIME NOT NULL,
        duration INTEGER,
        status TEXT DEFAULT 'Scheduled',
        createdAt DATETIME DEFAULT CURRENT_TIMESTAMP,
        updatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
        FOREIGN KEY (userId) REFERENCES users(id)
      )
    `);

    console.log('✓ Database tables created successfully');
    
    // Check if demo user exists
    const user = await db.get('SELECT * FROM users WHERE username = ?', 'demo');
    if (!user) {
      // Hash password: demo (for development only)
      const hashedPassword = '$2a$10$YF8L8dNpQhPqVK5JSPNqfe3.MFxqVG3xFSXQfUNGqFWXFzZ5XN7lC'; // bcrypt hash of 'demo'
      
      await db.run(
        'INSERT INTO users (username, email, password, fullName) VALUES (?, ?, ?, ?)',
        ['demo', 'demo@example.com', hashedPassword, 'Demo User']
      );
      console.log('✓ Demo user created (username: demo, password: demo)');
    }

    await db.close();
    console.log('✓ Database initialization complete');
  } catch (error) {
    console.error('Error initializing database:', error);
    process.exit(1);
  }
}

initializeDatabase();
