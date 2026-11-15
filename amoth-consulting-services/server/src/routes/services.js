import { getDb } from '../database/db.js';

const mapService = (service) => ({
  ...service,
  order: service.displayOrder
});

export async function getServicesRoute(fastify, options) {
  fastify.get('/services', async (request, reply) => {
    try {
      const db = getDb();
      const services = await db.all('SELECT * FROM services ORDER BY "displayOrder" ASC');
      return reply.send(services.map(mapService));
    } catch (error) {
      console.error('Get services error:', error);
      return reply.status(500).send({ error: 'Failed to get services' });
    }
  });
}

export async function getServiceRoute(fastify, options) {
  fastify.get('/services/:id', async (request, reply) => {
    try {
      const db = getDb();
      const service = await db.get(
        'SELECT * FROM services WHERE id = ?',
        [request.params.id]
      );

      if (!service) {
        return reply.status(404).send({ error: 'Service not found' });
      }

      return reply.send(mapService(service));
    } catch (error) {
      console.error('Get service error:', error);
      return reply.status(500).send({ error: 'Failed to get service' });
    }
  });
}
