import { getDb } from '../database/db.js';
import { authMiddleware } from '../middleware/auth.js';

export async function getProjectsRoute(fastify, options) {
  fastify.get('/projects', { onRequest: [authMiddleware] }, async (request, reply) => {
    try {
      const db = getDb();
      const projects = await db.all(
        'SELECT * FROM projects WHERE userId = ? ORDER BY createdAt DESC',
        [request.user.userId]
      );

      return reply.send(projects);
    } catch (error) {
      console.error('Get projects error:', error);
      return reply.status(500).send({ error: 'Failed to get projects' });
    }
  });
}

export async function getProjectRoute(fastify, options) {
  fastify.get('/projects/:id', { onRequest: [authMiddleware] }, async (request, reply) => {
    try {
      const db = getDb();
      const project = await db.get(
        'SELECT * FROM projects WHERE id = ? AND userId = ?',
        [request.params.id, request.user.userId]
      );

      if (!project) {
        return reply.status(404).send({ error: 'Project not found' });
      }

      // Get tasks for this project
      const tasks = await db.all(
        'SELECT * FROM tasks WHERE projectId = ? ORDER BY dueDate ASC',
        [request.params.id]
      );

      return reply.send({ ...project, tasks });
    } catch (error) {
      console.error('Get project error:', error);
      return reply.status(500).send({ error: 'Failed to get project' });
    }
  });
}

export async function createProjectRoute(fastify, options) {
  fastify.post('/projects', { onRequest: [authMiddleware] }, async (request, reply) => {
    try {
      const { name, description, status, startDate, endDate } = request.body;

      if (!name) {
        return reply.status(400).send({
          error: 'Project name is required'
        });
      }

      const db = getDb();

      const result = await db.run(
        'INSERT INTO projects (name, description, userId, status, startDate, endDate) VALUES (?, ?, ?, ?, ?, ?)',
        [name, description || '', request.user.userId, status || 'Active', startDate || null, endDate || null]
      );

      const project = await db.get(
        'SELECT * FROM projects WHERE id = ?',
        [result.lastID]
      );

      return reply.status(201).send({
        message: 'Project created successfully',
        project
      });
    } catch (error) {
      console.error('Create project error:', error);
      return reply.status(500).send({ error: 'Failed to create project' });
    }
  });
}

export async function updateProjectRoute(fastify, options) {
  fastify.put('/projects/:id', { onRequest: [authMiddleware] }, async (request, reply) => {
    try {
      const { name, description, status, completionRate, startDate, endDate } = request.body;
      const db = getDb();

      // Verify ownership
      const project = await db.get(
        'SELECT id FROM projects WHERE id = ? AND userId = ?',
        [request.params.id, request.user.userId]
      );

      if (!project) {
        return reply.status(404).send({ error: 'Project not found' });
      }

      const updateFields = [];
      const updateValues = [];

      if (name !== undefined) {
        updateFields.push('name = ?');
        updateValues.push(name);
      }
      if (description !== undefined) {
        updateFields.push('description = ?');
        updateValues.push(description);
      }
      if (status !== undefined) {
        updateFields.push('status = ?');
        updateValues.push(status);
      }
      if (completionRate !== undefined) {
        updateFields.push('completionRate = ?');
        updateValues.push(completionRate);
      }
      if (startDate !== undefined) {
        updateFields.push('startDate = ?');
        updateValues.push(startDate);
      }
      if (endDate !== undefined) {
        updateFields.push('endDate = ?');
        updateValues.push(endDate);
      }

      if (updateFields.length === 0) {
        return reply.status(400).send({ error: 'No fields to update' });
      }

      updateFields.push('updatedAt = CURRENT_TIMESTAMP');
      updateValues.push(request.params.id);

      await db.run(
        `UPDATE projects SET ${updateFields.join(', ')} WHERE id = ?`,
        updateValues
      );

      const updatedProject = await db.get(
        'SELECT * FROM projects WHERE id = ?',
        [request.params.id]
      );

      return reply.send({
        message: 'Project updated successfully',
        project: updatedProject
      });
    } catch (error) {
      console.error('Update project error:', error);
      return reply.status(500).send({ error: 'Failed to update project' });
    }
  });
}

export async function deleteProjectRoute(fastify, options) {
  fastify.delete('/projects/:id', { onRequest: [authMiddleware] }, async (request, reply) => {
    try {
      const db = getDb();

      // Verify ownership
      const project = await db.get(
        'SELECT id FROM projects WHERE id = ? AND userId = ?',
        [request.params.id, request.user.userId]
      );

      if (!project) {
        return reply.status(404).send({ error: 'Project not found' });
      }

      // Delete associated tasks
      await db.run(
        'DELETE FROM tasks WHERE projectId = ?',
        [request.params.id]
      );

      // Delete project
      await db.run(
        'DELETE FROM projects WHERE id = ?',
        [request.params.id]
      );

      return reply.send({
        message: 'Project deleted successfully'
      });
    } catch (error) {
      console.error('Delete project error:', error);
      return reply.status(500).send({ error: 'Failed to delete project' });
    }
  });
}
