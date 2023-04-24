<script setup lang="ts">
import 'leaflet/dist/leaflet.css';
import { LMap, LTileLayer } from '@vue-leaflet/vue-leaflet';


</script>

<template>
    <div style="height: 600px; width: 800px">
        <l-map :zoom="10" :center="[49.10, 9.18]" :use-global-leaflet="false">
            <l-tile-layer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" layer-type="base" name="OpenStreetMap" />
        </l-map>
    </div>
</template>