import { getDb } from '../database/db.js';
import { hashPassword } from '../utils/auth.js';
import { authMiddleware } from '../middleware/auth.js';

export async function getUserProfileRoute(fastify, options) {
  fastify.get('/profile', { onRequest: [authMiddleware] }, async (request, reply) => {
    try {
      const db = getDb();
      const user = await db.get(
        'SELECT id, username, email, fullName, bio, createdAt, updatedAt FROM users WHERE id = ?',
        [request.user.userId]
      );

      if (!user) {
        return reply.status(404).send({ error: 'User not found' });
      }

      return reply.send(user);
    } catch (error) {
      console.error('Get profile error:', error);
      return reply.status(500).send({ error: 'Failed to get profile' });
    }
  });
}

export async function updateUserProfileRoute(fastify, options) {
  fastify.put('/profile', { onRequest: [authMiddleware] }, async (request, reply) => {
    try {
      const { email, fullName, bio } = request.body;
      const db = getDb();

      // Check if email is already taken by another user
      if (email) {
        const existingUser = await db.get(
          'SELECT id FROM users WHERE email = ? AND id != ?',
          [email, request.user.userId]
        );

        if (existingUser) {
          return reply.status(409).send({ error: 'Email already in use' });
        }
      }

      const updateFields = [];
      const updateValues = [];

      if (email !== undefined) {
        updateFields.push('email = ?');
        updateValues.push(email);
      }
      if (fullName !== undefined) {
        updateFields.push('fullName = ?');
        updateValues.push(fullName);
      }
      if (bio !== undefined) {
        updateFields.push('bio = ?');
        updateValues.push(bio);
      }

      updateFields.push('updatedAt = CURRENT_TIMESTAMP');

      if (updateFields.length === 1) {
        return reply.status(400).send({ error: 'No fields to update' });
      }

      updateValues.push(request.user.userId);

      await db.run(
        `UPDATE users SET ${updateFields.join(', ')} WHERE id = ?`,
        updateValues
      );

      const user = await db.get(
        'SELECT id, username, email, fullName, bio, updatedAt FROM users WHERE id = ?',
        [request.user.userId]
      );

      return reply.send({
        message: 'Profile updated successfully',
        user
      });
    } catch (error) {
      console.error('Update profile error:', error);
      return reply.status(500).send({ error: 'Failed to update profile' });
    }
  });
}

export async function changePasswordRoute(fastify, options) {
  fastify.post('/change-password', { onRequest: [authMiddleware] }, async (request, reply) => {
    try {
      const { currentPassword, newPassword, confirmPassword } = request.body;

      if (!currentPassword || !newPassword || !confirmPassword) {
        return reply.status(400).send({
          error: 'Current password, new password, and confirmation are required'
        });
      }

      if (newPassword !== confirmPassword) {
        return reply.status(400).send({
          error: 'New passwords do not match'
        });
      }

      if (newPassword.length < 6) {
        return reply.status(400).send({
          error: 'Password must be at least 6 characters'
        });
      }

      const db = getDb();
      const user = await db.get(
        'SELECT password FROM users WHERE id = ?',
        [request.user.userId]
      );

      if (!user) {
        return reply.status(404).send({ error: 'User not found' });
      }

      // Verify current password
      const { comparePassword } = await import('../utils/auth.js');
      const passwordMatch = await comparePassword(currentPassword, user.password);

      if (!passwordMatch) {
        return reply.status(401).send({
          error: 'Current password is incorrect'
        });
      }

      // Hash and update new password
      const hashedPassword = await hashPassword(newPassword);
      await db.run(
        'UPDATE users SET password = ?, updatedAt = CURRENT_TIMESTAMP WHERE id = ?',
        [hashedPassword, request.user.userId]
      );

      return reply.send({
        message: 'Password changed successfully'
      });
    } catch (error) {
      console.error('Change password error:', error);
      return reply.status(500).send({ error: 'Failed to change password' });
    }
  });
}
