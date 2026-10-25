<template>
  <section class="completion-rate">
    <h2 v-if="showHeader">Completion Rate Dashboard</h2>

    <div class="summary-card">
      <div class="percentage" role="progressbar" :aria-valuenow="rate" aria-valuemin="0" aria-valuemax="100">{{ rate }}%</div>
      <div>
        <div class="label">Task completion</div>
        <div class="subtext">{{ completedCount }} of {{ taskCount }} tasks completed</div>
      </div>
    </div>

    <div class="calls">
      <button @click="openFull" class="btn-fulldashboard">View full project dashboard</button>
    </div>
  </section>
</template>

<script setup>
import { useRouter } from 'vue-router'

defineProps({
  showHeader: { type: Boolean, default: true },
  rate: { type: Number, default: 0 },
  completedCount: { type: Number, default: 0 },
  taskCount: { type: Number, default: 0 },
})

const router = useRouter()

const openFull = () => {
  router.push('/projects')
}
</script>

<style scoped>
.completion-rate {
  background: var(--color-background-soft);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  padding: 1.25rem;
  margin-top: 1.5rem;
  box-shadow: 0 6px 18px rgba(2, 6, 23, 0.04);
}
.completion-rate h2{margin-top:0;color:var(--color-heading);font-size:1.35rem}
.summary-card{display:flex;align-items:center;gap:1rem;background:var(--color-background);border:1px solid var(--color-border);border-radius:10px;padding:1rem}
.percentage{font-size:2.25rem;font-weight:800;color:var(--accent)}
.label{font-weight:600;color:var(--color-heading)}
.subtext{color:var(--color-text);opacity:.85;font-size:.9rem}
.calls{margin-top:1rem;display:flex;gap:.6rem;flex-wrap:wrap}
.btn-fulldashboard{border:0;border-radius:8px;padding:.45rem .9rem;font-weight:600;cursor:pointer}
.btn-fulldashboard{background:#0ea5e9;color:#fff}
.btn-fulldashboard:hover{background:#0284c7}
</style>
