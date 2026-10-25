import { getDb } from '../database/db.js';
import { generateToken, comparePassword, hashPassword, verifyToken } from '../utils/auth.js';
import { authMiddleware } from '../middleware/auth.js';
import { createCsrfToken, csrfCookieName, csrfCookieOptions } from '../middleware/csrf.js';
import { createMfaSecret, decryptSecret, encryptSecret, verifyMfaToken } from '../utils/mfa.js';

const sessionCookieOptions = (request) => ({
  httpOnly: true,
  secure: process.env.NODE_ENV === 'production',
  sameSite: 'lax',
  path: '/',
  maxAge: 60 * 60 * 24
});

function setSessionCookies(request, reply, token) {
  reply.setCookie('session', token, sessionCookieOptions(request));
  reply.setCookie(csrfCookieName, createCsrfToken(), csrfCookieOptions());
}

export async function registerRoute(fastify, options) {
  fastify.post('/register', {
    config: { rateLimit: { max: 5, timeWindow: '1 minute' } },
    schema: {
      body: {
        type: 'object',
        required: ['username', 'email', 'password'],
        additionalProperties: false,
        properties: {
          username: { type: 'string', minLength: 3, maxLength: 64, pattern: '^[A-Za-z0-9_.-]+$' },
          email: { type: 'string', format: 'email', maxLength: 254 },
          password: { type: 'string', minLength: 12, maxLength: 128 },
          fullName: { type: 'string', maxLength: 120 }
        }
      }
    }
  }, async (request, reply) => {
    try {
      const { username, email, password, fullName } = request.body;

      if (!username || !email || !password) {
        return reply.status(400).send({
          error: 'Username, email, and password are required'
        });
      }

      if (password.length < 12) {
        return reply.status(400).send({
          error: 'Password must be at least 12 characters'
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
      if (reply.setCookie) setSessionCookies(request, reply, token);

      return reply.status(201).send({
        message: 'User registered successfully',
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
  fastify.post('/login', {
    config: { rateLimit: { max: 5, timeWindow: '1 minute' } },
    schema: {
      body: {
        type: 'object',
        required: ['username', 'password'],
        additionalProperties: false,
        properties: {
          username: { type: 'string', minLength: 3, maxLength: 64 },
          password: { type: 'string', minLength: 1, maxLength: 128 },
          mfaToken: { type: 'string', pattern: '^\\d{6}$' }
        }
      }
    }
  }, async (request, reply) => {
    try {
      const { username, password, mfaToken } = request.body;

      if (!username || !password) {
        return reply.status(400).send({
          error: 'Username and password are required'
        });
      }

      const db = getDb();

      // Find user
      const user = await db.get(
        'SELECT id, username, email, password, fullName, bio, "mfaEnabled", "mfaSecret" FROM users WHERE username = ?',
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

      if (user.mfaEnabled) {
        if (!mfaToken || !user.mfaSecret || !(await verifyMfaToken(decryptSecret(user.mfaSecret), mfaToken))) {
          return reply.status(401).send({ error: 'MFA token required or invalid', mfaRequired: true });
        }
      }

      const token = generateToken(user.id, user.username);
      if (reply.setCookie) setSessionCookies(request, reply, token);

      return reply.send({
        message: 'Login successful',
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

export async function sessionRoute(fastify) {
  fastify.get('/session', async (request, reply) => {
    const user = request.cookies?.session ? verifyToken(request.cookies.session) : null;
    if (!user) return reply.status(401).send({ error: 'Not authenticated' });
    return reply.send({ user });
  });
}

export async function logoutRoute(fastify) {
  fastify.post('/logout', async (request, reply) => {
    reply.clearCookie('session', { path: '/' });
    reply.clearCookie(csrfCookieName, { path: '/' });
    return reply.send({ message: 'Logged out successfully' });
  });
}

export async function setupMfaRoute(fastify) {
  fastify.post('/mfa/setup', { onRequest: [authMiddleware] }, async (request, reply) => {
    const db = getDb();
    const { username } = await db.get('SELECT username FROM users WHERE id = ?', [request.user.userId]);
    const { secret, uri } = createMfaSecret(username);
    await db.run('UPDATE users SET "mfaSecret" = ?, "mfaEnabled" = FALSE WHERE id = ?', [encryptSecret(secret), request.user.userId]);
    return reply.send({ uri, message: 'Scan the URI with an authenticator app, then verify a token to enable MFA' });
  });
}

export async function enableMfaRoute(fastify) {
  fastify.post('/mfa/enable', {
    onRequest: [authMiddleware],
    schema: { body: { type: 'object', required: ['token'], additionalProperties: false, properties: { token: { type: 'string', pattern: '^\\d{6}$' } } } }
  }, async (request, reply) => {
    const db = getDb();
    const user = await db.get('SELECT "mfaSecret" FROM users WHERE id = ?', [request.user.userId]);
    if (!user?.mfaSecret || !(await verifyMfaToken(decryptSecret(user.mfaSecret), request.body.token))) return reply.status(400).send({ error: 'Invalid MFA token' });
    await db.run('UPDATE users SET "mfaEnabled" = TRUE WHERE id = ?', [request.user.userId]);
    return reply.send({ message: 'MFA enabled successfully' });
  });
}
