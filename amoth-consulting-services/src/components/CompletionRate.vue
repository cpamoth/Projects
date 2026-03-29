<template>
  <section class="completion-rate">
    <h2 v-if="showHeader">Completion Rate Dashboard</h2>

    <div class="summary-card">
      <div class="percentage">{{ overallRate }}%</div>
      <div class="label">Project completion</div>
      <div class="subtext">Based on tasks, meetings, and milestones</div>
    </div>

    <div class="rate-breakdown">
      <article v-for="item in items" :key="item.id" class="breakdown-item">
        <h3>{{ item.title }}</h3>
        <p>{{ item.description }}</p>
        <div class="dial-wrapper" role="progressbar" :aria-valuenow="item.rate" aria-valuemin="0" aria-valuemax="100">
          <div class="dial" :style="{ width: `${item.rate}%` }"></div>
        </div>
        <span class="item-rate">{{ item.rate }}%</span>
      </article>
    </div>

    <div class="calls">
      <button @click="refreshData" class="btn-refresh">Refresh metrics</button>
      <button @click="openFull" class="btn-fulldashboard">View full project dashboard</button>
    </div>
  </section>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const props = defineProps({
  showHeader: { type: Boolean, default: true }
})

const router = useRouter()

const overallRate = ref(95)
const items = ref([
  { id: 'cards', title: 'Active Projects', description: 'Percentage of active projects on schedule', rate: 89 },
  { id: 'tasks', title: 'Pending Tasks Completed', description: 'Pending task completion progress', rate: 78 },
  { id: 'meetings', title: 'On-time Meetings', description: 'Meetings held on schedule', rate: 91 },
  { id: 'quality', title: 'Quality Milestones', description: 'QA milestones passed', rate: 97 }
])

const refreshData = () => {
  // Simulated refresh for demo; replace with backend fetch
  overallRate.value = Math.min(100, Math.max(70, overallRate.value + (Math.random() * 8 - 4)))
  items.value = items.value.map(item => ({ ...item, rate: Math.min(100, Math.max(50, Math.round(item.rate + (Math.random() * 6 - 3)))) }))
}

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
.rate-breakdown{margin-top:1rem;display:grid;gap:.85rem}
.breakdown-item{padding:.8rem;border:1px solid var(--color-border);border-radius:10px;background:var(--color-background);}
.breakdown-item h3{margin:.0 .0 .35rem;font-size:1rem;color:var(--color-heading)}
.breakdown-item p{margin:0 0 .5rem;color:var(--color-text);font-size:.9rem}
.dial-wrapper{height:8px;background:var(--color-background-soft);border-radius:999px;overflow:hidden;margin-bottom:.4rem}
.dial{height:100%;background:linear-gradient(90deg,#2c5282,#0ea5e9);transition:width .3s ease}
.item-rate{font-size:.8rem;font-weight:700;color:var(--color-text);opacity:.85}
.calls{margin-top:1rem;display:flex;gap:.6rem;flex-wrap:wrap}
.btn-refresh,.btn-fulldashboard{border:0;border-radius:8px;padding:.45rem .9rem;font-weight:600;cursor:pointer}
.btn-refresh{background:#e2e8f0;color:#1e293b}
.btn-refresh:hover{background:#cbd5e1}
.btn-fulldashboard{background:#0ea5e9;color:#fff}
.btn-fulldashboard:hover{background:#0284c7}
</style>