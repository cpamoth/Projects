<script setup>
import NavBar from './components/NavBar.vue'
import { useRoute } from 'vue-router'

const route = useRoute()
</script>

<template>
  <a href="#main-content" class="skip-to-main">Skip to main content</a>
  <header>
    <NavBar />
  </header>

  <main id="main-content">
    <RouterView />
  </main>
</template>

<style>
:root { --navbar-height: 64px; }
header {
  line-height: 1.5;
}

.logo {
  display: block;
  margin: 0 auto 2rem;
}

main {
  padding-top: calc(var(--navbar-height) + 1rem);
}

/* Skip to main content link */
.skip-to-main {
  position: absolute;
  top: -40px;
  left: 0;
  background: #2b6cb0;
  color: white;
  padding: 8px;
  z-index: 100;
  text-decoration: none;
  border-radius: 0 0 4px 0;
}

.skip-to-main:focus {
  top: 0;
}

/* Focus indicators for accessibility */
a:focus, button:focus, input:focus, textarea:focus, select:focus {
  outline: 3px solid #2b6cb0;
  outline-offset: 2px;
}

@media (min-width: 1024px) {
  header {
    display: flex;
    place-items: center;
    padding-right: calc(var(--section-gap) / 2);
  }

  .logo {
    margin: 0 2rem 0 0;
  }

  header .wrapper {
    display: flex;
    place-items: flex-start;
    flex-wrap: wrap;
  }
}
</style>