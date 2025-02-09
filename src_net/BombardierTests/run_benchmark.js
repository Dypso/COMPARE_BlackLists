const { execSync } = require('child_process');
const scenarios = require('./test_scenarios');

async function runBenchmark() {
    console.log('Starting benchmark suite...\n');

    for (const scenario of scenarios) {
        console.log(`\nRunning scenario: ${scenario.name}`);
        console.log('Warming up...');
        
        // Warmup
        const warmupCommand = `bombardier -c ${scenario.concurrency} -d ${scenario.warmup} \
            -m POST -H "Content-Type: application/json" \
            -b '{"emvId":"1234567890123456"}' \
            http://blacklist-api/${scenario.endpoint}`;
        
        execSync(warmupCommand, { stdio: 'ignore' });

        console.log('Starting test...');
        
        // Actual test
        const testCommand = `bombardier -c ${scenario.concurrency} -d ${scenario.duration} \
            -m POST -H "Content-Type: application/json" \
            -b '{"emvId":"1234567890123456"}' \
            -p r -o json \
            http://blacklist-api/${scenario.endpoint}`;
        
        try {
            const output = execSync(testCommand, { encoding: 'utf8' });
            const results = JSON.parse(output);
            
            console.log('\nResults:');
            console.log(`Requests/sec: ${results.rps.mean.toFixed(2)}`);
            console.log(`Latency (mean): ${results.latency.mean.toFixed(2)}ms`);
            console.log(`Latency (p99): ${results.latency.p99.toFixed(2)}ms`);
            console.log(`Error rate: ${(results.errors / results.requests.total * 100).toFixed(2)}%`);
            
            // Cool down period
            await new Promise(resolve => setTimeout(resolve, 10000));
        } catch (error) {
            console.error(`Error running test: ${error.message}`);
        }
    }
}

runBenchmark().catch(console.error);