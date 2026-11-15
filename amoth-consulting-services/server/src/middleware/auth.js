import { verifyToken } from '../utils/auth.js';

export async function authMiddleware(request, reply) {
  try {
    const authorization = request.headers.authorization || '';
    const [scheme, token] = authorization.split(' ');
    const sessionToken = request.cookies?.session;
    const providedToken = sessionToken || (scheme === 'Bearer' && authorization.split(' ').length === 2 ? token : null);
    
    if (!providedToken) {
      return reply.status(401).send({ error: 'Missing authorization token' });
    }

    const decoded = verifyToken(providedToken);
    
    if (!decoded) {
      return reply.status(401).send({ error: 'Invalid or expired token' });
    }

    request.user = decoded;
  } catch (error) {
    return reply.status(401).send({ error: 'Authentication failed' });
  }
}
