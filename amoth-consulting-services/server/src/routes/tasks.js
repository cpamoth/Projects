import { getDb } from '../database/db.js';
import { authMiddleware } from '../middleware/auth.js';

export async function getTasksRoute(fastify, options) {
  fastify.get('/tasks', { onRequest: [authMiddleware] }, async (request, reply) => {
    try {
      const db = getDb();
      const projectId = request.query.projectId;

      let query = 'SELECT * FROM tasks WHERE userId = ?';
      const params = [request.user.userId];

      if (projectId) {
        query += ' AND projectId = ?';
        params.push(projectId);
      }

      query += ' ORDER BY dueDate ASC';

      const tasks = await db.all(query, params);

      return reply.send(tasks);
    } catch (error) {
      console.error('Get tasks error:', error);
      return reply.status(500).send({ error: 'Failed to get tasks' });
    }
  });
}

export async function getTaskRoute(fastify, options) {
  fastify.get('/tasks/:id', { onRequest: [authMiddleware] }, async (request, reply) => {
    try {
      const db = getDb();
      const task = await db.get(
        'SELECT * FROM tasks WHERE id = ? AND userId = ?',
        [request.params.id, request.user.userId]
      );

      if (!task) {
        return reply.status(404).send({ error: 'Task not found' });
      }

      return reply.send(task);
    } catch (error) {
      console.error('Get task error:', error);
      return reply.status(500).send({ error: 'Failed to get task' });
    }
  });
}

export async function createTaskRoute(fastify, options) {
  fastify.post('/tasks', { onRequest: [authMiddleware] }, async (request, reply) => {
    try {
      const { title, description, projectId, status, dueDate } = request.body;

      if (!title) {
        return reply.status(400).send({
          error: 'Task title is required'
        });
      }

      const db = getDb();

      // If projectId is provided, verify it belongs to the user
      if (projectId) {
        const project = await db.get(
          'SELECT id FROM projects WHERE id = ? AND userId = ?',
          [projectId, request.user.userId]
        );

        if (!project) {
          return reply.status(404).send({ error: 'Project not found' });
        }
      }

      const result = await db.run(
        'INSERT INTO tasks (title, description, projectId, userId, status, dueDate) VALUES (?, ?, ?, ?, ?, ?)',
        [title, description || '', projectId || null, request.user.userId, status || 'Pending', dueDate || null]
      );

      const task = await db.get(
        'SELECT * FROM tasks WHERE id = ?',
        [result.lastID]
      );

      return reply.status(201).send({
        message: 'Task created successfully',
        task
      });
    } catch (error) {
      console.error('Create task error:', error);
      return reply.status(500).send({ error: 'Failed to create task' });
    }
  });
}

export async function updateTaskRoute(fastify, options) {
  fastify.put('/tasks/:id', { onRequest: [authMiddleware] }, async (request, reply) => {
    try {
      const { title, description, status, dueDate, completed } = request.body;
      const db = getDb();

      // Verify ownership
      const task = await db.get(
        'SELECT id FROM tasks WHERE id = ? AND userId = ?',
        [request.params.id, request.user.userId]
      );

      if (!task) {
        return reply.status(404).send({ error: 'Task not found' });
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
      if (status !== undefined) {
        updateFields.push('status = ?');
        updateValues.push(status);
      }
      if (dueDate !== undefined) {
        updateFields.push('dueDate = ?');
        updateValues.push(dueDate);
      }
      if (completed !== undefined) {
        updateFields.push('completed = ?');
        updateValues.push(completed ? 1 : 0);
      }

      if (updateFields.length === 0) {
        return reply.status(400).send({ error: 'No fields to update' });
      }

      updateFields.push('updatedAt = CURRENT_TIMESTAMP');
      updateValues.push(request.params.id);

      await db.run(
        `UPDATE tasks SET ${updateFields.join(', ')} WHERE id = ?`,
        updateValues
      );

      const updatedTask = await db.get(
        'SELECT * FROM tasks WHERE id = ?',
        [request.params.id]
      );

      return reply.send({
        message: 'Task updated successfully',
        task: updatedTask
      });
    } catch (error) {
      console.error('Update task error:', error);
      return reply.status(500).send({ error: 'Failed to update task' });
    }
  });
}

export async function deleteTaskRoute(fastify, options) {
  fastify.delete('/tasks/:id', { onRequest: [authMiddleware] }, async (request, reply) => {
    try {
      const db = getDb();

      // Verify ownership
      const task = await db.get(
        'SELECT id FROM tasks WHERE id = ? AND userId = ?',
        [request.params.id, request.user.userId]
      );

      if (!task) {
        return reply.status(404).send({ error: 'Task not found' });
      }

      await db.run(
        'DELETE FROM tasks WHERE id = ?',
        [request.params.id]
      );

      return reply.send({
        message: 'Task deleted successfully'
      });
    } catch (error) {
      console.error('Delete task error:', error);
      return reply.status(500).send({ error: 'Failed to delete task' });
    }
  });
}
