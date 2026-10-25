import crypto from 'node:crypto';
import dotenv from 'dotenv';
import { generateSecret, generateURI, verify } from 'otplib';

dotenv.config();

const encryptionKey = process.env.MFA_ENCRYPTION_KEY;
if (process.env.NODE_ENV === 'production' && (!encryptionKey || !/^[a-f0-9]{64}$/i.test(encryptionKey))) {
  throw new Error('MFA_ENCRYPTION_KEY must be a 64-character hex key in production');
}

const key = Buffer.from(encryptionKey || crypto.createHash('sha256').update('development-mfa-key').digest('hex'), 'hex');

export function encryptSecret(secret) {
  const iv = crypto.randomBytes(12);
  const cipher = crypto.createCipheriv('aes-256-gcm', key, iv);
  const encrypted = Buffer.concat([cipher.update(secret, 'utf8'), cipher.final()]);
  return `${iv.toString('hex')}:${cipher.getAuthTag().toString('hex')}:${encrypted.toString('hex')}`;
}

export function decryptSecret(value) {
  const [ivHex, tagHex, encryptedHex] = value.split(':');
  const decipher = crypto.createDecipheriv('aes-256-gcm', key, Buffer.from(ivHex, 'hex'));
  decipher.setAuthTag(Buffer.from(tagHex, 'hex'));
  return Buffer.concat([decipher.update(Buffer.from(encryptedHex, 'hex')), decipher.final()]).toString('utf8');
}

export function createMfaSecret(username) {
  const secret = generateSecret();
  const uri = generateURI({ issuer: 'Amoth Consulting Services', label: username, secret });
  return { secret, uri };
}

export async function verifyMfaToken(secret, token) {
  const result = await verify({ secret, token: String(token) });
  return result.valid;
}