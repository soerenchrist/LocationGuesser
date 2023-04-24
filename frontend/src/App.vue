<script setup lang="ts">
  import Header from './components/common/Header.vue';


</script>

<template>
  <div class="h-screen bg-teal-800 text-center text-white">
    <Header />
    <RouterView />
  </div>
</template>