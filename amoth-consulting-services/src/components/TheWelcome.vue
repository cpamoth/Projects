<script setup>
const openReadmeInEditor = () => fetch('/__open-in-editor?file=README.md')
</script>

<template>
	<PageWrapper>
		<div class="the-welcome">
			<slot />
		</div>
	</PageWrapper>
</template>
