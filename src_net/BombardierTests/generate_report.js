const fs = require('fs');
const path = require('path');

function generateReport(results) {
    const report = {
        timestamp: new Date().toISOString(),
        summary: {
            totalScenarios: results.length,
            bestPerformer: null,
            worstPerformer: null
        },
        scenarios: results,
        comparisons: {
            throughput: [],
            latency: [],
            errorRate: []
        }
    };

    // Sort scenarios by performance metrics
    report.comparisons.throughput = results
        .sort((a, b) => b.rps.mean - a.rps.mean)
        .map(r => ({
            name: r.name,
            rps: r.rps.mean
        }));

    report.comparisons.latency = results
        .sort((a, b) => a.latency.p99 - b.latency.p99)
        .map(r => ({
            name: r.name,
            p99: r.latency.p99
        }));

    report.comparisons.errorRate = results
        .sort((a, b) => a.errorRate - b.errorRate)
        .map(r => ({
            name: r.name,
            errorRate: r.errorRate
        }));

    // Determine best and worst performers
    report.summary.bestPerformer = report.comparisons.throughput[0];
    report.summary.worstPerformer = report.comparisons.throughput[report.comparisons.throughput.length - 1];

    // Generate markdown report
    const markdown = generateMarkdownReport(report);
    
    // Save reports
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
    fs.writeFileSync(`benchmark-report-${timestamp}.json`, JSON.stringify(report, null, 2));
    fs.writeFileSync(`benchmark-report-${timestamp}.md`, markdown);
}

function generateMarkdownReport(report) {
    return `# Blacklist Validation Methods Benchmark Report

## Summary
- Date: ${report.timestamp}
- Total Scenarios: ${report.summary.totalScenarios}
- Best Performer: ${report.summary.bestPerformer.name} (${report.summary.bestPerformer.rps.toFixed(2)} req/sec)
- Worst Performer: ${report.summary.worstPerformer.name} (${report.summary.worstPerformer.rps.toFixed(2)} req/sec)

## Throughput Comparison
${report.comparisons.throughput.map(r => `- ${r.name}: ${r.rps.toFixed(2)} req/sec`).join('\n')}

## Latency Comparison (p99)
${report.comparisons.latency.map(r => `- ${r.name}: ${r.p99.toFixed(2)}ms`).join('\n')}

## Error Rates
${report.comparisons.errorRate.map(r => `- ${r.name}: ${r.errorRate.toFixed(2)}%`).join('\n')}

## Detailed Results

${report.scenarios.map(s => `### ${s.name}
- Throughput: ${s.rps.mean.toFixed(2)} req/sec
- Latency (mean): ${s.latency.mean.toFixed(2)}ms
- Latency (p99): ${s.latency.p99.toFixed(2)}ms
- Error Rate: ${s.errorRate.toFixed(2)}%`).join('\n\n')}
`;
}

module.exports = { generateReport };