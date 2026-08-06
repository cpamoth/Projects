import { getDb } from '../database/db.js';
import { authMiddleware } from '../middleware/auth.js';

export async function getMeetingsRoute(fastify, options) {
  fastify.get('/meetings', { onRequest: [authMiddleware] }, async (request, reply) => {
    try {
      const db = getDb();
      const meetings = await db.all(
        'SELECT * FROM meetings WHERE userId = ? ORDER BY scheduledDate DESC',
        [request.user.userId]
      );

      return reply.send(meetings);
    } catch (error) {
      console.error('Get meetings error:', error);
      return reply.status(500).send({ error: 'Failed to get meetings' });
    }
  });
}

export async function getMeetingRoute(fastify, options) {
  fastify.get('/meetings/:id', { onRequest: [authMiddleware] }, async (request, reply) => {
    try {
      const db = getDb();
      const meeting = await db.get(
        'SELECT * FROM meetings WHERE id = ? AND userId = ?',
        [request.params.id, request.user.userId]
      );

      if (!meeting) {
        return reply.status(404).send({ error: 'Meeting not found' });
      }

      return reply.send(meeting);
    } catch (error) {
      console.error('Get meeting error:', error);
      return reply.status(500).send({ error: 'Failed to get meeting' });
    }
  });
}

export async function createMeetingRoute(fastify, options) {
  fastify.post('/meetings', { onRequest: [authMiddleware] }, async (request, reply) => {
    try {
      const { title, description, scheduledDate, duration, status } = request.body;

      if (!title || !scheduledDate) {
        return reply.status(400).send({
          error: 'Title and scheduled date are required'
        });
      }

      const db = getDb();

      const result = await db.run(
        'INSERT INTO meetings (title, description, userId, scheduledDate, duration, status) VALUES (?, ?, ?, ?, ?, ?)',
        [title, description || '', request.user.userId, scheduledDate, duration || 60, status || 'Scheduled']
      );

      const meeting = await db.get(
        'SELECT * FROM meetings WHERE id = ?',
        [result.lastID]
      );

      return reply.status(201).send({
        message: 'Meeting created successfully',
        meeting
      });
    } catch (error) {
      console.error('Create meeting error:', error);
      return reply.status(500).send({ error: 'Failed to create meeting' });
    }
  });
}

export async function updateMeetingRoute(fastify, options) {
  fastify.put('/meetings/:id', { onRequest: [authMiddleware] }, async (request, reply) => {
    try {
      const { title, description, scheduledDate, duration, status } = request.body;
      const db = getDb();

      // Verify ownership
      const meeting = await db.get(
        'SELECT id FROM meetings WHERE id = ? AND userId = ?',
        [request.params.id, request.user.userId]
      );

      if (!meeting) {
        return reply.status(404).send({ error: 'Meeting not found' });
      }

      const updateFields = [];
      const updateValues = [];

      if (title !== undefined) {
        updateFields.push('title = ?');
        updateValues.push(title);
      }
      if (description !== undefined) {
        updateFields.push('description = ?');
        updateValues.push(description);
      }
      if (scheduledDate !== undefined) {
        updateFields.push('scheduledDate = ?');
        updateValues.push(scheduledDate);
      }
      if (duration !== undefined) {
        updateFields.push('duration = ?');
        updateValues.push(duration);
      }
      if (status !== undefined) {
        updateFields.push('status = ?');
        updateValues.push(status);
      }

      if (updateFields.length === 0) {
        return reply.status(400).send({ error: 'No fields to update' });
      }

      updateFields.push('updatedAt = CURRENT_TIMESTAMP');
      updateValues.push(request.params.id);

      await db.run(
        `UPDATE meetings SET ${updateFields.join(', ')} WHERE id = ?`,
        updateValues
      );

      const updatedMeeting = await db.get(
        'SELECT * FROM meetings WHERE id = ?',
        [request.params.id]
      );

      return reply.send({
        message: 'Meeting updated successfully',
        meeting: updatedMeeting
      });
    } catch (error) {
      console.error('Update meeting error:', error);
      return reply.status(500).send({ error: 'Failed to update meeting' });
    }
  });
}

export async function deleteMeetingRoute(fastify, options) {
  fastify.delete('/meetings/:id', { onRequest: [authMiddleware] }, async (request, reply) => {
    try {
      const db = getDb();

      // Verify ownership
      const meeting = await db.get(
        'SELECT id FROM meetings WHERE id = ? AND userId = ?',
        [request.params.id, request.user.userId]
      );

      if (!meeting) {
        return reply.status(404).send({ error: 'Meeting not found' });
      }

      await db.run(
        'DELETE FROM meetings WHERE id = ?',
        [request.params.id]
      );

      return reply.send({
        message: 'Meeting deleted successfully'
      });
    } catch (error) {
      console.error('Delete meeting error:', error);
      return reply.status(500).send({ error: 'Failed to delete meeting' });
    }
  });
}
