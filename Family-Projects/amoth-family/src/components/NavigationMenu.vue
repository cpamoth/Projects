<template>
    <nav :style="{background: background || '#333'}">
  
    </nav>
  </template>
  
  <script>
  import { defineComponent } from '@vue/composition-api'
  
  export default defineComponent({
    setup() {
      
    },
  })
  </script>
  
  <style scoped>
    nav {
      height: 60px;
      width: 100px;
    }
  </style>
  