import { getDb } from '../database/db.js';
import { generateToken, comparePassword, hashPassword } from '../utils/auth.js';

export async function registerRoute(fastify, options) {
  fastify.post('/register', async (request, reply) => {
    try {
      const { username, email, password, fullName } = request.body;

      if (!username || !email || !password) {
        return reply.status(400).send({
          error: 'Username, email, and password are required'
        });
      }

      const db = getDb();

      // Check if user exists
      const existingUser = await db.get(
        'SELECT id FROM users WHERE username = ? OR email = ?',
        [username, email]
      );

      if (existingUser) {
        return reply.status(409).send({
          error: 'Username or email already exists'
        });
      }

      // Hash password
      const hashedPassword = await hashPassword(password);

      // Create user
      const result = await db.run(
        'INSERT INTO users (username, email, password, fullName) VALUES (?, ?, ?, ?)',
        [username, email, hashedPassword, fullName || '']
      );

      const token = generateToken(result.lastID, username);

      return reply.status(201).send({
        message: 'User registered successfully',
        token,
        user: {
          id: result.lastID,
          username,
          email,
          fullName: fullName || ''
        }
      });
    } catch (error) {
      console.error('Register error:', error);
      return reply.status(500).send({ error: 'Registration failed' });
    }
  });
}

export async function loginRoute(fastify, options) {
  fastify.post('/login', async (request, reply) => {
    try {
      const { username, password } = request.body;

      if (!username || !password) {
        return reply.status(400).send({
          error: 'Username and password are required'
        });
      }

      const db = getDb();

      // Find user
      const user = await db.get(
        'SELECT id, username, email, password, fullName, bio FROM users WHERE username = ?',
        [username]
      );

      if (!user) {
        return reply.status(401).send({
          error: 'Invalid username or password'
        });
      }

      // Compare passwords
      const passwordMatch = await comparePassword(password, user.password);

      if (!passwordMatch) {
        return reply.status(401).send({
          error: 'Invalid username or password'
        });
      }

      const token = generateToken(user.id, user.username);

      return reply.send({
        message: 'Login successful',
        token,
        user: {
          id: user.id,
          username: user.username,
          email: user.email,
          fullName: user.fullName,
          bio: user.bio
        }
      });
    } catch (error) {
      console.error('Login error:', error);
      return reply.status(500).send({ error: 'Login failed' });
    }
  });
}
