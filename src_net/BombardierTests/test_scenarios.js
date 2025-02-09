const scenarios = [
    {
        name: 'Bloom Filter - Low Concurrency',
        endpoint: '/api/bloomfilter/validate',
        concurrency: 100,
        duration: '30s',
        warmup: '5s'
    },
    {
        name: 'Bloom Filter - Medium Concurrency',
        endpoint: '/api/bloomfilter/validate',
        concurrency: 1000,
        duration: '30s',
        warmup: '5s'
    },
    {
        name: 'Bloom Filter - High Concurrency',
        endpoint: '/api/bloomfilter/validate',
        concurrency: 10000,
        duration: '30s',
        warmup: '10s'
    },
    {
        name: 'Cuckoo Filter - Low Concurrency',
        endpoint: '/api/cuckoofilter/validate',
        concurrency: 100,
        duration: '30s',
        warmup: '5s'
    },
    {
        name: 'Cuckoo Filter - Medium Concurrency',
        endpoint: '/api/cuckoofilter/validate',
        concurrency: 1000,
        duration: '30s',
        warmup: '5s'
    },
    {
        name: 'Cuckoo Filter - High Concurrency',
        endpoint: '/api/cuckoofilter/validate',
        concurrency: 10000,
        duration: '30s',
        warmup: '10s'
    },
    {
        name: 'UltraFast BitArray - Low Concurrency',
        endpoint: '/api/ultrafast/validate',
        concurrency: 100,
        duration: '30s',
        warmup: '5s'
    },
    {
        name: 'UltraFast BitArray - Medium Concurrency',
        endpoint: '/api/ultrafast/validate',
        concurrency: 1000,
        duration: '30s',
        warmup: '5s'
    },
    {
        name: 'UltraFast BitArray - High Concurrency',
        endpoint: '/api/ultrafast/validate',
        concurrency: 10000,
        duration: '30s',
        warmup: '10s'
    },
    {
        name: 'Two-Level Cache - Low Concurrency',
        endpoint: '/api/twolevelcache/validate',
        concurrency: 100,
        duration: '30s',
        warmup: '5s'
    },
    {
        name: 'Two-Level Cache - Medium Concurrency',
        endpoint: '/api/twolevelcache/validate',
        concurrency: 1000,
        duration: '30s',
        warmup: '5s'
    },
    {
        name: 'Two-Level Cache - High Concurrency',
        endpoint: '/api/twolevelcache/validate',
        concurrency: 10000,
        duration: '30s',
        warmup: '10s'
    }
];

module.exports = scenarios;