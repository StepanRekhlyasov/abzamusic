<template>
  <q-layout view="lHh Lpr lFf" class="app-layout">
    <q-header elevated>
      <q-toolbar class="app-header">
        <q-toolbar-title> Abzamusic </q-toolbar-title>
        <div>Abzamusic v1.0.0</div>
      </q-toolbar>
    </q-header>
    <q-page-container>
      <router-view />
    </q-page-container>
  </q-layout>
</template>

<style lang="scss">
.app-layout {
  --app-header-height: 50px;
}

.app-header {
  min-height: var(--app-header-height);
  height: var(--app-header-height);
}
</style>
