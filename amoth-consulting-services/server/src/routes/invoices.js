import { getDb } from '../database/db.js';
import { authMiddleware } from '../middleware/auth.js';

export async function getInvoicesRoute(fastify, options) {
  fastify.get('/invoices', { onRequest: [authMiddleware] }, async (request, reply) => {
    try {
      const db = getDb();
      const invoices = await db.all(
        'SELECT * FROM invoices WHERE userId = ? ORDER BY dueDate DESC',
        [request.user.userId]
      );

      return reply.send(invoices);
    } catch (error) {
      console.error('Get invoices error:', error);
      return reply.status(500).send({ error: 'Failed to get invoices' });
    }
  });
}

export async function getInvoiceRoute(fastify, options) {
  fastify.get('/invoices/:id', { onRequest: [authMiddleware] }, async (request, reply) => {
    try {
      const db = getDb();
      const invoice = await db.get(
        'SELECT * FROM invoices WHERE id = ? AND userId = ?',
        [request.params.id, request.user.userId]
      );

      if (!invoice) {
        return reply.status(404).send({ error: 'Invoice not found' });
      }

      return reply.send(invoice);
    } catch (error) {
      console.error('Get invoice error:', error);
      return reply.status(500).send({ error: 'Failed to get invoice' });
    }
  });
}

export async function createInvoiceRoute(fastify, options) {
  fastify.post('/invoices', { onRequest: [authMiddleware] }, async (request, reply) => {
    try {
      const { invoiceNumber, amount, dueDate, description } = request.body;

      if (!invoiceNumber || !amount || !dueDate) {
        return reply.status(400).send({
          error: 'Invoice number, amount, and due date are required'
        });
      }

      const db = getDb();

      // Check if invoice number already exists
      const existingInvoice = await db.get(
        'SELECT id FROM invoices WHERE invoiceNumber = ?',
        [invoiceNumber]
      );

      if (existingInvoice) {
        return reply.status(409).send({
          error: 'Invoice number already exists'
        });
      }

      const result = await db.run(
        'INSERT INTO invoices (invoiceNumber, userId, amount, dueDate, description) VALUES (?, ?, ?, ?, ?)',
        [invoiceNumber, request.user.userId, amount, dueDate, description || '']
      );

      const invoice = await db.get(
        'SELECT * FROM invoices WHERE id = ?',
        [result.lastID]
      );

      return reply.status(201).send({
        message: 'Invoice created successfully',
        invoice
      });
    } catch (error) {
      console.error('Create invoice error:', error);
      return reply.status(500).send({ error: 'Failed to create invoice' });
    }
  });
}

export async function updateInvoiceRoute(fastify, options) {
  fastify.put('/invoices/:id', { onRequest: [authMiddleware] }, async (request, reply) => {
    try {
      const { status, amount, dueDate, description } = request.body;
      const db = getDb();

      // Verify ownership
      const invoice = await db.get(
        'SELECT id FROM invoices WHERE id = ? AND userId = ?',
        [request.params.id, request.user.userId]
      );

      if (!invoice) {
        return reply.status(404).send({ error: 'Invoice not found' });
      }

      const updateFields = [];
      const updateValues = [];

      if (status !== undefined) {
        updateFields.push('status = ?');
        updateValues.push(status);
      }
      if (amount !== undefined) {
        updateFields.push('amount = ?');
        updateValues.push(amount);
      }
      if (dueDate !== undefined) {
        updateFields.push('dueDate = ?');
        updateValues.push(dueDate);
      }
      if (description !== undefined) {
        updateFields.push('description = ?');
        updateValues.push(description);
      }

      if (updateFields.length === 0) {
        return reply.status(400).send({ error: 'No fields to update' });
      }

      updateFields.push('updatedAt = CURRENT_TIMESTAMP');
      updateValues.push(request.params.id);

      await db.run(
        `UPDATE invoices SET ${updateFields.join(', ')} WHERE id = ?`,
        updateValues
      );

      const updatedInvoice = await db.get(
        'SELECT * FROM invoices WHERE id = ?',
        [request.params.id]
      );

      return reply.send({
        message: 'Invoice updated successfully',
        invoice: updatedInvoice
      });
    } catch (error) {
      console.error('Update invoice error:', error);
      return reply.status(500).send({ error: 'Failed to update invoice' });
    }
  });
}

export async function deleteInvoiceRoute(fastify, options) {
  fastify.delete('/invoices/:id', { onRequest: [authMiddleware] }, async (request, reply) => {
    try {
      const db = getDb();

      // Verify ownership
      const invoice = await db.get(
        'SELECT id FROM invoices WHERE id = ? AND userId = ?',
        [request.params.id, request.user.userId]
      );

      if (!invoice) {
        return reply.status(404).send({ error: 'Invoice not found' });
      }

      await db.run(
        'DELETE FROM invoices WHERE id = ?',
        [request.params.id]
      );

      return reply.send({
        message: 'Invoice deleted successfully'
      });
    } catch (error) {
      console.error('Delete invoice error:', error);
      return reply.status(500).send({ error: 'Failed to delete invoice' });
    }
  });
}
