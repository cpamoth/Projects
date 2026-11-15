import bcrypt from 'bcryptjs';
import { services } from '../../../src/data/services.js';
import { closeDb, getDb, initDb } from './db.js';
import { runMigrations } from './migrate.js';

async function initializeDatabase() {
  try {
    const pool = await initDb();
    const db = getDb();

    console.log('Initializing database...');

    await runMigrations(pool);
    console.log('✓ Database migrations applied successfully');
    
    if (process.env.NODE_ENV === 'development' && process.env.SEED_DEMO_USER === 'true') {
      const user = await db.get('SELECT * FROM users WHERE username = ?', ['demo']);
      if (!user) {
        const hashedPassword = await bcrypt.hash(process.env.DEMO_PASSWORD || 'demo', 12);

        await db.run(
          'INSERT INTO users (username, email, password, fullName) VALUES (?, ?, ?, ?)',
          ['demo', 'demo@example.com', hashedPassword, 'Demo User']
        );
        console.log('Demo user created for development only');
      }
    }

    for (const [displayOrder, service] of services.entries()) {
      await db.run(
        `INSERT INTO services (id, title, summary, highlights, details, approach, benefits, "displayOrder")
         VALUES (?, ?, ?, ?, ?, ?, ?, ?)
         ON CONFLICT (id) DO UPDATE SET
           title = EXCLUDED.title,
           summary = EXCLUDED.summary,
           highlights = EXCLUDED.highlights,
           details = EXCLUDED.details,
           approach = EXCLUDED.approach,
           benefits = EXCLUDED.benefits,
           "displayOrder" = EXCLUDED."displayOrder"`,
        [
          service.id,
          service.title,
          service.summary,
          JSON.stringify(service.highlights),
          service.details,
          service.approach,
          JSON.stringify(service.benefits),
          displayOrder
        ]
      );
    }

    await closeDb();
    console.log('✓ Database initialization complete');
  } catch (error) {
    console.error('Error initializing database:', error);
    await closeDb();
    process.exit(1);
  }
}

initializeDatabase();
