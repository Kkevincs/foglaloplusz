// Services page functionality
document.addEventListener("DOMContentLoaded", () => {
    loadServices();
});

async function loadServices() {
    try {
        const services = await getServices();
        const container = document.getElementById('serviceContainer');
        
        if (!container) return;
        
        container.innerHTML = '';
        
        services.forEach(service => {
            const name = service.name ?? service.Name ?? '';
            const description = service.description ?? service.Description ?? '';
            const price = service.price ?? service.Price ?? 0;
            
            container.innerHTML += `
                <div class="service-detail">
                    <div class="service-icon">💅</div>
                    <h2>${name}</h2>
                    <p>${description}</p>
                    <p><strong>${price} Ft</strong></p>
                </div>
            `;
        });
    } catch (error) {
        console.error("Failed to load services:", error);
        const container = document.getElementById('serviceContainer');
        if (container) {
            container.innerHTML = '<p style="text-align: center; color: red;">Failed to load services. Please refresh the page.</p>';
        }
    }
}
