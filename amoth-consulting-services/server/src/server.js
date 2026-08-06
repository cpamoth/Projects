import Fastify from 'fastify';
import fastifyCors from 'fastify-cors';
import dotenv from 'dotenv';
import { initDb, closeDb } from './database/db.js';

// Import routes
import { registerRoute, loginRoute } from './routes/auth.js';
import { getUserProfileRoute, updateUserProfileRoute, changePasswordRoute } from './routes/profile.js';
import { getInvoicesRoute, getInvoiceRoute, createInvoiceRoute, updateInvoiceRoute, deleteInvoiceRoute } from './routes/invoices.js';
import { getProjectsRoute, getProjectRoute, createProjectRoute, updateProjectRoute, deleteProjectRoute } from './routes/projects.js';
import { getTasksRoute, getTaskRoute, createTaskRoute, updateTaskRoute, deleteTaskRoute } from './routes/tasks.js';
import { getMeetingsRoute, getMeetingRoute, createMeetingRoute, updateMeetingRoute, deleteMeetingRoute } from './routes/meetings.js';

dotenv.config();

const PORT = process.env.PORT || 3001;
const NODE_ENV = process.env.NODE_ENV || 'development';

const fastify = Fastify({
  logger: true
});

// Register CORS
await fastify.register(fastifyCors, {
  origin: true,
  credentials: true
});

// Initialize database
await initDb();

// Health check route
fastify.get('/health', async (request, reply) => {
  return { status: 'ok' };
});

// Auth routes (public)
await fastify.register(registerRoute, { prefix: '/api/auth' });
await fastify.register(loginRoute, { prefix: '/api/auth' });

// Profile routes (protected)
await fastify.register(getUserProfileRoute, { prefix: '/api' });
await fastify.register(updateUserProfileRoute, { prefix: '/api' });
await fastify.register(changePasswordRoute, { prefix: '/api' });

// Invoice routes (protected)
await fastify.register(getInvoicesRoute, { prefix: '/api' });
await fastify.register(getInvoiceRoute, { prefix: '/api' });
await fastify.register(createInvoiceRoute, { prefix: '/api' });
await fastify.register(updateInvoiceRoute, { prefix: '/api' });
await fastify.register(deleteInvoiceRoute, { prefix: '/api' });

// Project routes (protected)
await fastify.register(getProjectsRoute, { prefix: '/api' });
await fastify.register(getProjectRoute, { prefix: '/api' });
await fastify.register(createProjectRoute, { prefix: '/api' });
await fastify.register(updateProjectRoute, { prefix: '/api' });
await fastify.register(deleteProjectRoute, { prefix: '/api' });

// Task routes (protected)
await fastify.register(getTasksRoute, { prefix: '/api' });
await fastify.register(getTaskRoute, { prefix: '/api' });
await fastify.register(createTaskRoute, { prefix: '/api' });
await fastify.register(updateTaskRoute, { prefix: '/api' });
await fastify.register(deleteTaskRoute, { prefix: '/api' });

// Meeting routes (protected)
await fastify.register(getMeetingsRoute, { prefix: '/api' });
await fastify.register(getMeetingRoute, { prefix: '/api' });
await fastify.register(createMeetingRoute, { prefix: '/api' });
await fastify.register(updateMeetingRoute, { prefix: '/api' });
await fastify.register(deleteMeetingRoute, { prefix: '/api' });

// Graceful shutdown
const gracefulShutdown = async (signal) => {
  console.log(`\nReceived ${signal}, gracefully shutting down...`);
  await fastify.close();
  await closeDb();
  process.exit(0);
};

process.on('SIGTERM', () => gracefulShutdown('SIGTERM'));
process.on('SIGINT', () => gracefulShutdown('SIGINT'));

// Start server
const start = async () => {
  try {
    await fastify.listen({ port: PORT, host: '0.0.0.0' });
    console.log(`\n✓ Server running at http://localhost:${PORT}`);
    console.log(`✓ Environment: ${NODE_ENV}`);
    console.log(`✓ API Endpoints available at http://localhost:${PORT}/api\n`);
  } catch (err) {
    fastify.log.error(err);
    process.exit(1);
  }
};

start();

export default fastify;
