<template>
  <PageWrapper>
    <section class="services">
      <div class="container">
        <h1 class="green">Our Services</h1>
        <p class="intro">We partner with organizations to deliver practical, measurable outcomes across technology, product, and operations.</p>

        <div class="cards">
          <RouterLink
            v-for="service in services"
            :key="service.id"
            :to="`/services/${service.id}`"
            class="card"
          >
            <h3>{{ service.title }}</h3>
            <p>{{ service.summary }}</p>
            <ul class="service-highlights">
              <li v-for="highlight in service.highlights" :key="highlight">{{ highlight }}</li>
            </ul>
          </RouterLink>
        </div>

      </div>
    </section>
  </PageWrapper>
</template>

<script setup>
import { services } from '../data/services'
</script>

<style scoped>
.services { max-width: 1100px; margin: 0 auto; padding: 2rem 1rem; }
.container { text-align: center; }
.intro { margin-top: .5rem; color: var(--color-text); opacity: .92 }
.cards { margin-top: 1.25rem; display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 1rem; }
.card { display: block; background: var(--color-background-soft); border: 1px solid var(--color-border); border-radius: 10px; padding: 1rem; box-shadow: 0 6px 18px rgba(2,6,23,0.04); text-align: left; color: inherit; text-decoration: none; transition: transform 0.2s ease, box-shadow 0.2s ease; }
.card:hover,
.card:focus-visible { transform: translateY(-2px); box-shadow: 0 10px 24px rgba(2,6,23,0.1); }
.card h3 { margin: 0 0 .5rem; color: var(--color-heading); font-size: 1.05rem }
.card p { margin: 0 0 1rem; color: var(--color-text); opacity: .9; font-size: .95rem }
.service-highlights { list-style: disc inside; margin: 0; padding: 0; color: var(--color-text); opacity: .85; font-size: .92rem; }
.service-highlights li { margin-bottom: 0.5rem; }

@media (min-width: 1024px) {
  .services { padding: 2rem 0 }
}
</style>
