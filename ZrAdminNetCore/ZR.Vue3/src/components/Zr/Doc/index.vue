<template>
  <div>
    <svg-icon name="question" @click="goto" />
  </div>
</template>
<script setup>
const url = ref('https://www.szlyuan.com//doc')

function goto() {
  window.open(url.value)
}
</script>
