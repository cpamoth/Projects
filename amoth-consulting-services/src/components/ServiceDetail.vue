<template>
  <PageWrapper>
    <section class="service-detail">
      <div class="container">
        <RouterLink to="/services" class="back-link">← Back to services</RouterLink>

        <h1 class="green">{{ service.title }}</h1>
        <p class="intro">{{ service.summary }}</p>

        <div class="details">
          <p>{{ service.details }}</p>
        </div>
      </div>
    </section>
  </PageWrapper>
</template>

<script setup>
import { useRoute, useRouter } from 'vue-router'
import { services } from '../data/services'

const route = useRoute()
const router = useRouter()
const serviceId = route.params.serviceId
const service = services.find((item) => item.id === serviceId)

if (!service) {
  router.replace('/services')
}
</script>

<style scoped>
.service-detail { max-width: 900px; margin: 2rem auto; padding: 1rem }
.container { text-align: left }
.back-link { display: inline-block; margin-bottom: 1rem; color: var(--accent); text-decoration: none; font-weight: 600 }
.back-link:hover,
.back-link:focus-visible { text-decoration: underline }
.intro { margin-top: .5rem; color: var(--color-text); opacity: .92 }
.details { background: var(--color-background-soft); border: 1px solid var(--color-border); border-radius: 10px; padding: 1.5rem; box-shadow: 0 6px 18px rgba(2,6,23,0.04); margin-top: 1.5rem }
.details p { margin: 0; color: var(--color-text); line-height: 1.75 }
</style>
