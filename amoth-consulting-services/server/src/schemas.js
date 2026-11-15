const idParams = {
  type: 'object',
  required: ['id'],
  additionalProperties: false,
  properties: { id: { type: 'integer', minimum: 1 } }
};

const text = (maxLength = 1000) => ({ type: 'string', maxLength });
const date = { type: 'string', format: 'date' };
const dateTime = { type: 'string', format: 'date-time' };
const status = (values) => ({ type: 'string', enum: values });
const partial = (properties) => ({ type: 'object', additionalProperties: false, properties });

const schemas = {
  invoices: {
    create: { body: { type: 'object', required: ['invoiceNumber', 'amount', 'dueDate'], additionalProperties: false, properties: { invoiceNumber: text(80), amount: { type: 'number', exclusiveMinimum: 0 }, dueDate: date, description: text() } } },
    update: { params: idParams, body: partial({ status: status(['Pending', 'Paid', 'Overdue']), amount: { type: 'number', exclusiveMinimum: 0 }, dueDate: date, description: text() }) },
    id: { params: idParams }
  },
  projects: {
    create: { body: { type: 'object', required: ['name'], additionalProperties: false, properties: { name: text(160), description: text(), status: status(['Active', 'Completed', 'On Hold']), startDate: date, endDate: date } } },
    update: { params: idParams, body: partial({ name: text(160), description: text(), status: status(['Active', 'Completed', 'On Hold']), completionRate: { type: 'number', minimum: 0, maximum: 100 }, startDate: date, endDate: date }) },
    id: { params: idParams }
  },
  tasks: {
    create: { body: { type: 'object', required: ['title'], additionalProperties: false, properties: { title: text(160), description: text(), projectId: { type: 'integer', minimum: 1 }, status: status(['Pending', 'In Progress', 'Completed']), dueDate: date } } },
    update: { params: idParams, body: partial({ title: text(160), description: text(), status: status(['Pending', 'In Progress', 'Completed']), dueDate: date, completed: { type: 'boolean' } }) },
    id: { params: idParams },
    list: { querystring: { type: 'object', additionalProperties: false, properties: { projectId: { type: 'integer', minimum: 1 } } } }
  },
  meetings: {
    create: { body: { type: 'object', required: ['title', 'scheduledDate'], additionalProperties: false, properties: { title: text(160), description: text(), scheduledDate: dateTime, duration: { type: 'integer', minimum: 1, maximum: 1440 }, status: status(['Scheduled', 'Completed', 'Cancelled']) } } },
    update: { params: idParams, body: partial({ title: text(160), description: text(), scheduledDate: dateTime, duration: { type: 'integer', minimum: 1, maximum: 1440 }, status: status(['Scheduled', 'Completed', 'Cancelled']) }) },
    id: { params: idParams }
  },
  profile: {
    update: { body: partial({ email: { type: 'string', format: 'email', maxLength: 254 }, fullName: text(120), bio: text(2000) }) }
  }
};

export function registerRouteSchemas(fastify) {
  fastify.addHook('onRoute', (routeOptions) => {
    const method = Array.isArray(routeOptions.method) ? routeOptions.method[0] : routeOptions.method;
    const url = routeOptions.url;
    let schema;

    if (url.includes('/invoices')) schema = method === 'POST' ? schemas.invoices.create : method === 'PUT' ? schemas.invoices.update : url.includes(':id') ? schemas.invoices.id : undefined;
    if (url.includes('/projects')) schema = method === 'POST' ? schemas.projects.create : method === 'PUT' ? schemas.projects.update : url.includes(':id') ? schemas.projects.id : undefined;
    if (url.includes('/tasks')) schema = method === 'POST' ? schemas.tasks.create : method === 'PUT' ? schemas.tasks.update : url.includes(':id') ? schemas.tasks.id : method === 'GET' ? schemas.tasks.list : undefined;
    if (url.includes('/meetings')) schema = method === 'POST' ? schemas.meetings.create : method === 'PUT' ? schemas.meetings.update : url.includes(':id') ? schemas.meetings.id : undefined;
    if (url === '/profile' && method === 'PUT') schema = schemas.profile.update;

    if (schema) routeOptions.schema = { ...routeOptions.schema, ...schema };
  });
}