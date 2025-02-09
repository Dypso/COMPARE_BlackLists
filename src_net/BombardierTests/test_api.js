const fs = require('fs');
const { execSync } = require('child_process');
const { generateReport } = require('./generate_report');

const config = JSON.parse(fs.readFileSync('test_config.json', 'utf8'));
const results = [];

async function runTests() {
    console.log('Starting performance tests...\n');

    for (const scenario of config.scenarios) {
        console.log(`\nTesting ${scenario.name}...`);
        const scenarioResults = {
            name: scenario.name,
            rps: { mean: 0 },
            latency: { mean: 0, p99: 0 },
            errorRate: 0,
            results: []
        };

        for (const concurrency of scenario.concurrencyLevels) {
            console.log(`\nTesting with ${concurrency} concurrent connections`);
            
            for (const payload of scenario.payloads) {
                console.log(`\nTesting payload type: ${payload.type}`);
                
                // Warmup
                console.log('Warming up...');
                const warmupCommand = `bombardier \
                    -c ${concurrency} \
                    -d 5s \
                    -m ${scenario.method} \
                    -H "Content-Type: application/json" \
                    -b '${JSON.stringify({ emvId: payload.emvId })}' \
                    http://blacklist-api:80${scenario.endpoint}`;
                
                execSync(warmupCommand, { stdio: 'ignore' });

                // Actual test
                console.log('Running test...');
                const testCommand = `bombardier \
                    -c ${concurrency} \
                    -d ${scenario.duration} \
                    -m ${scenario.method} \
                    -H "Content-Type: application/json" \
                    -b '${JSON.stringify({ emvId: payload.emvId })}' \
                    -p r -o json \
                    http://blacklist-api:80${scenario.endpoint}`;
                
                try {
                    const output = execSync(testCommand, { encoding: 'utf8' });
                    const result = JSON.parse(output);
                    
                    scenarioResults.results.push({
                        concurrency,
                        payload: payload.type,
                        rps: result.rps.mean,
                        latency: {
                            mean: result.latency.mean,
                            p99: result.latency.p99
                        },
                        errorRate: (result.errors / result.requests.total) * 100
                    });
                    
                    console.log('\nResults:');
                    console.log(`Requests/sec: ${result.rps.mean.toFixed(2)}`);
                    console.log(`Latency (mean): ${result.latency.mean.toFixed(2)}ms`);
                    console.log(`Latency (p99): ${result.latency.p99.toFixed(2)}ms`);
                    console.log(`Error rate: ${((result.errors / result.requests.total) * 100).toFixed(2)}%`);
                    
                    // Cool down period
                    await new Promise(resolve => setTimeout(resolve, 10000));
                } catch (error) {
                    console.error(`Error running test: ${error.message}`);
                }
            }
        }

        // Calculate averages for the scenario
        if (scenarioResults.results.length > 0) {
            scenarioResults.rps.mean = scenarioResults.results.reduce((acc, r) => acc + r.rps, 0) / scenarioResults.results.length;
            scenarioResults.latency.mean = scenarioResults.results.reduce((acc, r) => acc + r.latency.mean, 0) / scenarioResults.results.length;
            scenarioResults.latency.p99 = scenarioResults.results.reduce((acc, r) => acc + r.latency.p99, 0) / scenarioResults.results.length;
            scenarioResults.errorRate = scenarioResults.results.reduce((acc, r) => acc + r.errorRate, 0) / scenarioResults.results.length;
        }

        results.push(scenarioResults);
    }

    // Generate final report
    generateReport(results);
}

runTests().catch(console.error);