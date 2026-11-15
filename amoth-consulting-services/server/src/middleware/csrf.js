import crypto from 'node:crypto';

export const csrfCookieName = 'csrf';
export const csrfHeaderName = 'x-csrf-token';

export function createCsrfToken() {
  return crypto.randomBytes(32).toString('hex');
}

export function csrfCookieOptions() {
  return { httpOnly: false, secure: process.env.NODE_ENV === 'production', sameSite: 'lax', path: '/', maxAge: 60 * 60 * 24 };
}

export async function csrfMiddleware(request, reply) {
  const unsafeMethod = !['GET', 'HEAD', 'OPTIONS'].includes(request.method);
  const hasCookieAuth = Boolean(request.cookies?.session) && !request.headers.authorization;
  if (!unsafeMethod || !hasCookieAuth) return;

  const cookieToken = request.cookies?.[csrfCookieName];
  const headerToken = request.headers[csrfHeaderName];
  if (!cookieToken || !headerToken || cookieToken.length !== headerToken.length || !crypto.timingSafeEqual(Buffer.from(cookieToken), Buffer.from(headerToken))) {
    return reply.status(403).send({ error: 'CSRF validation failed' });
  }
}