<script setup lang="ts">
import StatsCard from '../common/StatsCard.vue';

defineProps<{
    distance: number,
    yearsOff: number,
    totalPoints: number,
    roundPoints: {
        yearPoints: number,
        distancePoints: number
    }
}>()

const formatDistance = (distance: number) => {
    if (distance > 100000) {
        return `${(distance / 1000).toFixed(0)} km`;
    }
    if (distance > 1000) {
        return `${(distance / 1000).toFixed(2)} km`;
    }

    return `${distance.toFixed(0)} m`;
}

</script>

<template>
    <div class="grid grid-cols-3 gap-4 p-4">
        <StatsCard title="Distance" :value="formatDistance(distance)" />
        <StatsCard title="Years off" :value="yearsOff.toString()" />
        <StatsCard title="Total points" :value="totalPoints.toString()" />
        <StatsCard title="Distance points" :value="`${roundPoints.distancePoints}/5000`" />
        <StatsCard title="Year points" :value="`${roundPoints.yearPoints}/5000`" />
        <StatsCard title="Round points" :value="`${roundPoints.distancePoints + roundPoints.yearPoints}/10000`" />
    </div>
</template>