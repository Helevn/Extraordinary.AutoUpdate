// 简单的 API 层：负责与服务器交互（fetch）
// 可在此处集中处理 URL、错误处理、超时、mock 等

export async function fetchConfig() {
    const res = await fetch('/OriginConfig.json', { cache: 'no-store' });
    if (!res.ok) throw new Error('配置文件加载失败');
    return res.json();
}

export async function fetchFileListFromApi() {
    // 如果后端提供 /api/files 可以替换为真实端点
    const res = await fetch('/api/files', { cache: 'no-store' });
    if (!res.ok) throw new Error('无法获取文件列表');
    return res.json();
}

export async function simulateFileScan() {
    // 保持与原页面一致的模拟数据（当后端不可用时）
    return new Promise(resolve => {
        setTimeout(() => {
            const data = [
                { name: 'Extraordinary.App.exe', size: '25.8 MB', modified: '2023-10-26 14:30:22' },
                { name: 'WPFClient-1.0.2-d0baf410bc050324c62bb5f2e7e0b7d3.zip', size: '48.7 MB', modified: '2023-10-26 14:28:15' },
                { name: 'WPFClient-1.0.1-a1b2c3d4e5f678901234567890123456.zip', size: '47.9 MB', modified: '2023-09-15 11:42:33' },
                { name: 'WPFClient-1.0.0-0987654321abcdef1234567890abcdef.zip', size: '46.3 MB', modified: '2023-08-01 09:15:47' },
                { name: 'update-instructions.pdf', size: '1.2 MB', modified: '2023-10-20 16:22:18' },
                { name: 'checksums.txt', size: '0.8 KB', modified: '2023-10-26 14:30:45' }
            ];
            resolve(data);
        }, 800);
    });
}