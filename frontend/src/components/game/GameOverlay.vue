<script setup lang="ts">

defineProps<{
    totalPoints: number,
    round: number
}>();

</script>

<template>
    <div class="absolute left-6 top-20  rounded-lg bg-black opacity-60 w-40 p-4">
        <div class="text-white text-center">
            <div class="text-2xl font-bold">Round {{ round }}</div>
            <div class="text-4xl font-bold">{{ totalPoints }}</div>
        </div>
    </div>
</template>