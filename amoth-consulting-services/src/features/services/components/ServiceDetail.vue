<template>
  <PageWrapper>
    <section class="service-detail">
      <div class="container">
        <RouterLink to="/services" class="back-link">
          <span class="back-icon">←</span>
          <span>Back to services</span>
        </RouterLink>

        <h1 class="green">{{ service?.title || 'Service not found' }}</h1>
        <p class="intro">{{ service?.summary || 'The requested service could not be found.' }}</p>

        <div class="details" v-if="service?.details">
          <p>{{ service.details }}</p>
        </div>

        <div class="service-highlights" v-if="service?.highlights && service.highlights.length">
          <h2>Key highlights</h2>
          <ul>
            <li v-for="(item, index) in service.highlights" :key="index">{{ item }}</li>
          </ul>
        </div>

        <div class="service-approach" v-if="service?.approach">
          <h2>Our approach</h2>
          <p>{{ service.approach }}</p>
        </div>

        <div class="service-benefits" v-if="service?.benefits && service.benefits.length">
          <h2>What you get</h2>
          <ul>
            <li v-for="(item, index) in service.benefits" :key="index">{{ item }}</li>
          </ul>
        </div>
      </div>
    </section>
  </PageWrapper>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { services } from '../../../data/services.js'

const route = useRoute()
const router = useRouter()
const serviceId = computed(() => route.params.serviceId)
const service = computed(() => {
  const found = services.find((item) => item.id === serviceId.value)
  return found || services[0]
})

if (!service.value) {
  router.replace('/services')
}
</script>

<style scoped>
.service-detail { width: 100%; margin: 0; padding: .5rem }
.container { text-align: left }
.back-link { display: inline-flex; align-items: center; gap: 0.5rem; margin-bottom: 1rem; padding: 0.85rem 1.2rem; color: var(--accent); background: rgba(103, 217, 121, 0.1); border: 1px solid transparent; border-radius: 999px; text-decoration: none; font-weight: 700; letter-spacing: 0.01em; transition: transform 220ms cubic-bezier(0.22, 1, 0.36, 1), background-color 220ms ease, border-color 220ms ease, box-shadow 220ms ease; will-change: transform; }
.back-link:hover,
.back-link:focus-visible { background: rgba(103, 217, 121, 0.18); border-color: rgba(103, 217, 121, 0.4); box-shadow: 0 10px 24px rgba(103, 217, 121, 0.12); transform: translateY(-1px); }
.back-icon { display: inline-flex; align-items: center; justify-content: center; width: 1.4rem; height: 1.4rem; border-radius: 50%; background: rgba(103, 217, 121, 0.18); font-size: 0.95rem; transition: transform 220ms cubic-bezier(0.22, 1, 0.36, 1); will-change: transform; }
.back-link:hover .back-icon { transform: translateX(-4px) scale(1.05); }
.back-link span:last-child { display: inline-flex; transition: transform 220ms cubic-bezier(0.22, 1, 0.36, 1); }
.back-link:hover span:last-child { transform: translateX(-1px); }
.intro { margin-top: .5rem; color: var(--color-text); opacity: .92 }
.details,
.service-highlights,
.service-approach,
.service-benefits { background: var(--color-background-soft); border: 1px solid var(--color-border); border-radius: 10px; padding: 1.5rem; box-shadow: 0 6px 18px rgba(2,6,23,0.04); margin-top: 1.5rem }
.details p,
.service-approach p { margin: 0; color: var(--color-text); line-height: 1.75 }
.service-highlights h2,
.service-approach h2,
.service-benefits h2 { margin-top: 0; color: var(--color-text) }
.service-highlights ul,
.service-benefits ul { margin: 0; padding-left: 1.25rem; color: var(--color-text); line-height: 1.75 }
.service-highlights li,
.service-benefits li { margin-bottom: 0.75rem }
</style>
