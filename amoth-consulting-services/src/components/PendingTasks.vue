<template>
  <section class="pending-tasks">
    <h2>Pending Tasks</h2>

    <div v-if="tasks.length === 0" class="empty">No pending tasks currently. Great job!</div>

    <ul v-else class="tasks-list" aria-label="Pending tasks list">
      <li v-for="task in tasks" :key="task.id" class="task-item">
        <div>
          <strong>{{ task.title }}</strong>
          <p class="task-meta">{{ task.project }} • Due {{ task.dueDate }}</p>
        </div>
        <div class="task-actions">
          <span class="priority" :class="task.priority.toLowerCase()">{{ task.priority }}</span>
          <button @click="markDone(task.id)" class="btn-done">Mark done</button>
        </div>
      </li>
    </ul>
  </section>
</template>

<script setup>
import { ref } from 'vue'

const tasks = ref([
  { id: 101, title: 'Finalize user stories', project: 'Platform migration', dueDate: '2026-04-07', priority: 'High' },
  { id: 102, title: 'Deploy staging environment', project: 'Digital transformation', dueDate: '2026-04-12', priority: 'Medium' },
  { id: 103, title: 'Security policy review', project: 'Security roadmap', dueDate: '2026-04-14', priority: 'High' },
  { id: 104, title: 'Stakeholder feedback session', project: 'Operational analytics', dueDate: '2026-04-18', priority: 'Low' }
])

function markDone(taskId) {
  tasks.value = tasks.value.filter(task => task.id !== taskId)
}
</script>

<style scoped>
.pending-tasks {
  background: var(--color-background-soft);
  border: 1px solid var(--color-border);
  border-radius: 10px;
  padding: 1.25rem;
  margin-top: 1.5rem;
  box-shadow: 0 6px 18px rgba(2, 6, 23, 0.04);
}

.pending-tasks h2 {
  margin-top: 0;
  font-size: 1.35rem;
  color: var(--color-heading);
}

.empty {
  color: var(--color-text);
  padding: 1rem;
  background: var(--color-background);
  border: 1px solid var(--color-border);
  border-radius: 6px;
}

.tasks-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: grid;
  gap: 0.75rem;
}

.task-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border: 1px solid var(--color-border);
  border-radius: 10px;
  padding: 0.75rem;
  gap: 1rem;
  background: var(--color-background);
}

.task-meta {
  margin: 0.2rem 0 0;
  color: #666;
  font-size: 0.9rem;
}

.task-actions {
  display: flex;
  align-items: center;
  gap: 0.6rem;
}

.priority {
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
  border: 1px solid transparent;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
}

.priority.low { background: #e6f4ff; color: #1d4ed8; border-color: #bae0ff; }
.priority.medium { background: #fff7e6; color: #b57b00; border-color: #ffe2a8; }
.priority.high { background: #ffe8e8; color: #b02a1d; border-color: #feb6b6; }

.btn-done {
  border: none;
  background: #2b6cb0;
  color: white;
  padding: 0.4rem 0.8rem;
  border-radius: 5px;
  cursor: pointer;
  transition: background 0.2s ease;
}

.btn-done:hover {
  background: #1e4f7f;
}
</style>
