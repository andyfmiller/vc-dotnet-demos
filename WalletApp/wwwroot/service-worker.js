// WalletApp Service Worker
const CACHE_NAME = 'walletapp-v1';

// Core shell assets to pre-cache on install
const PRECACHE_ASSETS = [
    '/',
    '/css/site.min.css',
    '/js/site.js',
    '/lib/bootstrap/css/bootstrap.css',
    '/lib/bootstrap/js/bootstrap.bundle.js',
    '/lib/jquery/jquery.js',
    '/images/logo.png',
    '/manifest.json'
];

// Install: pre-cache the shell
self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => cache.addAll(PRECACHE_ASSETS))
            .then(() => self.skipWaiting())
    );
});

// Activate: delete old caches
self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(keys =>
            Promise.all(
                keys.filter(key => key !== CACHE_NAME)
                    .map(key => caches.delete(key))
            )
        ).then(() => self.clients.claim())
    );
});

// Fetch strategy:
//   - Static assets (css/js/images/fonts): cache-first
//   - Navigation (HTML pages): network-first with cache fallback
self.addEventListener('fetch', event => {
    const { request } = event;
    const url = new URL(request.url);

    // Only handle same-origin requests
    if (url.origin !== self.location.origin) return;

    const isStaticAsset =
        url.pathname.startsWith('/css/') ||
        url.pathname.startsWith('/js/') ||
        url.pathname.startsWith('/lib/') ||
        url.pathname.startsWith('/images/') ||
        url.pathname.startsWith('/fonts/');

    if (isStaticAsset) {
        // Cache-first for static assets
        event.respondWith(
            caches.match(request).then(cached => {
                if (cached) return cached;
                return fetch(request).then(response => {
                    if (response.ok) {
                        const clone = response.clone();
                        caches.open(CACHE_NAME).then(cache => cache.put(request, clone));
                    }
                    return response;
                });
            })
        );
    } else if (request.mode === 'navigate') {
        // Network-first for navigation — always get fresh pages
        event.respondWith(
            fetch(request).catch(() =>
                caches.match(request).then(cached => cached || caches.match('/'))
            )
        );
    }
});
