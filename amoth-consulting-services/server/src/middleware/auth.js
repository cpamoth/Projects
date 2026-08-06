import { verifyToken } from '../utils/auth.js';

export async function authMiddleware(request, reply) {
  try {
    const token = request.headers.authorization?.replace('Bearer ', '');
    
    if (!token) {
      return reply.status(401).send({ error: 'Missing authorization token' });
    }

    const decoded = verifyToken(token);
    
    if (!decoded) {
      return reply.status(401).send({ error: 'Invalid or expired token' });
    }

    request.user = decoded;
  } catch (error) {
    return reply.status(401).send({ error: 'Authentication failed' });
  }
}
