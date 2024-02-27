import http from 'k6/http';
import { sleep } from 'k6';
import { Counter } from 'k6/metrics';

let metrics200 = new Counter('metrics_200');
let metrics429 = new Counter('metrics_429');

export const options = {
    vus: 1,
    duration: '30s',
};

export default function () {
    const res = http.get('http://localhost:5239/sliding-window');

    switch (res.status) {
        case 200:
            metrics200.add(1);
            console.log(`[VU: ${__VU}][STATUS: ${res.status}] ${res.body}`);
        break;
        case 429:
            metrics429.add(1);
            console.error(`[VU: ${__VU}][STATUS: ${res.status}]`);
            break;
    }

    sleep(1);
}
