import { state, setConfig, setFileList } from './state.js';
import { fetchConfig, fetchFileListFromApi, simulateFileScan } from './api.js';

// UI 操作、DOM 渲染与交互逻辑放在此模块

export function initTabs() {
    const tabs = document.querySelectorAll('.tab-button');
    const contents = document.querySelectorAll('.tab-content');

    tabs.forEach(tab => {
        tab.addEventListener('click', function() {
            const target = this.id.replace('tab-', 'content-');

            tabs.forEach(t => t.classList.remove('active'));
            contents.forEach(c => c.classList.remove('active'));

            this.classList.add('active');
            document.getElementById(target).classList.add('active');
        });
    });
}

export function initSearch() {
    const searchInput = document.getElementById('file-search');
    if (!searchInput) return;

    searchInput.addEventListener('input', function() {
        const query = this.value.toLowerCase();

        if (query.length === 0) {
            renderFileList(state.fileList);
            return;
        }

        const filtered = state.fileList.filter(file =>
            file.name.toLowerCase().includes(query) ||
            file.size.toLowerCase().includes(query)
        );

        renderFileList(filtered);
    });
}

export function renderFileList(files) {
    const tbody = document.getElementById('file-list');
    if (!tbody) return;

    const loadingRow = document.getElementById('loading-row');
    if (loadingRow) loadingRow.remove();

    if (!files || files.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="4" class="px-6 py-12 text-center text-gray-500">
                    <i class="fas fa-folder-open text-3xl mb-3"></i>
                    <div>没有找到文件</div>
                </td>
            </tr>
        `;
        return;
    }

    tbody.innerHTML = '';
    files.forEach(file => {
        const isZip = file.name.endsWith('.zip');
        const isExe = file.name.endsWith('.exe');

        const row = document.createElement('tr');
        row.className = 'file-item hover:bg-gray-50';
        row.innerHTML = `
            <td class="px-6 py-4 whitespace-nowrap">
                <div class="flex items-center">
                    <div class="flex-shrink-0 h-10 w-10 ${isZip ? 'bg-yellow-100' : isExe ? 'bg-green-100' : 'bg-blue-100'} rounded-lg flex items-center justify-center">
                        <i class="fas ${isZip ? 'fa-file-archive text-yellow-600' : isExe ? 'fa-cogs text-green-600' : 'fa-file text-blue-600'}"></i>
                    </div>
                    <div class="ml-4">
                        <div class="text-sm font-medium text-gray-900">${file.name}</div>
                        <div class="text-sm text-gray-500">
                            ${isZip ? '压缩包' : isExe ? '应用程序' : '文档'}
                        </div>
                    </div>
                </div>
            </td>
            <td class="px-6 py-4 whitespace-nowrap">
                <div class="text-sm text-gray-900 font-medium">${file.size}</div>
            </td>
            <td class="px-6 py-4 whitespace-nowrap">
                <div class="text-sm text-gray-500">${file.modified}</div>
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                <button data-download="${file.name}" class="btn-download bg-indigo-600 hover:bg-indigo-700 text-white px-4 py-2 rounded-lg mr-2">
                    <i class="fas fa-download mr-1"></i>下载
                </button>
                ${isZip || isExe ? `
                <button data-info="${file.name}" class="btn-info border border-gray-300 text-gray-700 hover:bg-gray-50 px-4 py-2 rounded-lg">
                    <i class="fas fa-info-circle mr-1"></i>详情
                </button>` : ''}
            </td>
        `;
        tbody.appendChild(row);
    });

    // 绑定新生成按钮事件（事件委托也可）
    tbody.querySelectorAll('.btn-download').forEach(btn => {
        btn.addEventListener('click', () => downloadFile(btn.dataset.download));
    });
    tbody.querySelectorAll('.btn-info').forEach(btn => {
        btn.addEventListener('click', () => showFileInfo(btn.dataset.info));
    });
}

export async function loadConfigToUI() {
    try {
        const config = await fetchConfig();
        setConfig(config);
        document.getElementById('current-version').innerHTML = `
            ${config.AppVersion} 
            <span class="text-lg font-normal text-gray-600">(${config.AppMD5Version.substring(0, 8)}...)</span>
        `;
        document.getElementById('latest-version').textContent = config.AppVersion;
        document.getElementById('latest-md5').textContent = `MD5: ${config.AppMD5Version}`;
        document.getElementById('latest-package').innerHTML = `
            <i class="fas fa-file-archive mr-1"></i>
            包文件: <span class="font-medium">${config.PackageName}</span>
        `;
        document.getElementById('config-content').textContent = JSON.stringify(config, null, 4);

        const now = new Date();
        document.getElementById('last-update').textContent =
            `${now.getFullYear()}-${String(now.getMonth()+1).padStart(2,'0')}-${String(now.getDate()).padStart(2,'0')} ${String(now.getHours()).padStart(2,'0')}:${String(now.getMinutes()).padStart(2,'0')}`;
    } catch (err) {
        console.error('加载配置失败:', err);
        showNotification('错误', '无法加载配置文件', 'error');
    }
}

export async function loadFileListToUI() {
    try {
        const loadingRow = document.getElementById('loading-row');
        if (loadingRow) {
            loadingRow.innerHTML = `
                <td colspan="4" class="px-6 py-12 text-center">
                    <div class="flex flex-col items-center">
                        <div class="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-indigo-500 mb-4"></div>
                        <div class="text-gray-500">正在扫描文件...</div>
                    </div>
                </td>
            `;
        }

        let files = [];
        try {
            files = await fetchFileListFromApi();
        } catch {
            // 后端未提供时使用模拟
            files = await simulateFileScan();
        }

        setFileList(files);
        renderFileList(files);
        updateFileStats();
    } catch (err) {
        console.error('加载文件列表失败:', err);
        showNotification('错误', '无法加载文件列表', 'error');
    }
}

export function updateFileStats() {
    document.getElementById('file-count').textContent = state.fileList.length;

    let totalMB = 0;
    state.fileList.forEach(file => {
        const size = file.size;
        if (size.includes('MB')) {
            totalMB += parseFloat(size);
        } else if (size.includes('GB')) {
            totalMB += parseFloat(size) * 1024;
        } else if (size.includes('KB')) {
            totalMB += parseFloat(size) / 1024;
        }
    });

    document.getElementById('total-size').textContent = totalMB.toFixed(1) + ' MB';
    document.getElementById('client-count').textContent = Math.floor(Math.random() * 100) + 25;
}

export function bindStaticButtons() {
    // 顶部刷新
    const refreshBtn = document.getElementById('refresh-btn');
    if (refreshBtn) refreshBtn.addEventListener('click', refreshAll);

    // 页面级按钮
    document.querySelectorAll('.action-download').forEach(btn => {
        btn.addEventListener('click', () => downloadFile(btn.dataset.filename));
    });

    const viewCfg = document.getElementById('btn-view-config');
    if (viewCfg) viewCfg.addEventListener('click', () => document.getElementById('tab-config').click());

    const openApi = document.getElementById('btn-open-api');
    if (openApi) openApi.addEventListener('click', openApiDocs);

    const checkBtn = document.getElementById('check-updates-btn');
    if (checkBtn) checkBtn.addEventListener('click', checkForUpdates);

    const downloadLatestBtn = document.getElementById('btn-download-latest');
    if (downloadLatestBtn) downloadLatestBtn.addEventListener('click', downloadLatest);

    const copyMD5Btn = document.getElementById('btn-copy-md5');
    if (copyMD5Btn) copyMD5Btn.addEventListener('click', copyMD5);

    const copyCfgBtn = document.getElementById('btn-copy-config');
    if (copyCfgBtn) copyCfgBtn.addEventListener('click', copyConfig);
}

// 交互函数
export function downloadFile(filename) {
    showNotification('下载开始', `正在下载 ${filename}`, 'info');
    setTimeout(() => {
        showNotification('下载完成', `${filename} 下载成功`, 'success');
    }, 1500);
    // window.location.href = `/${filename}`;
}

export function downloadLatest() {
    if (!state.currentConfig) return;
    downloadFile(state.currentConfig.PackageName);
}

export function copyConfig() {
    if (!state.currentConfig) return;
    const configText = JSON.stringify(state.currentConfig, null, 4);
    navigator.clipboard.writeText(configText).then(() => {
        showNotification('已复制', '配置文件已复制到剪贴板', 'success');
    }).catch(err => {
        showNotification('错误', '复制失败: ' + err, 'error');
    });
}

export function copyMD5() {
    if (!state.currentConfig) return;
    navigator.clipboard.writeText(state.currentConfig.AppMD5Version).then(() => {
        showNotification('已复制', 'MD5 已复制到剪贴板', 'success');
    }).catch(err => {
        showNotification('错误', '复制失败: ' + err, 'error');
    });
}

export function showFileInfo(filename) {
    const file = state.fileList.find(f => f.name === filename);
    if (!file) return;
    showNotification('文件信息', `${filename} (${file.size})`, 'info');
}

export function checkForUpdates() {
    showNotification('检查更新', '正在扫描新版本...', 'info');
    setTimeout(() => {
        showNotification('更新检查完成', '当前已是最新版本', 'success');
    }, 2000);
}

export async function refreshAll() {
    const refreshBtn = document.getElementById('refresh-btn');
    const icon = refreshBtn ? refreshBtn.querySelector('.refresh-icon') : null;
    if (icon) icon.classList.add('refreshing');

    try {
        await Promise.all([
            loadConfigToUI(),
            loadFileListToUI()
        ]);
        showNotification('刷新完成', '数据已更新', 'success');
    } catch {
        showNotification('刷新失败', '部分数据加载失败', 'error');
    } finally {
        if (icon) setTimeout(() => icon.classList.remove('refreshing'), 500);
    }
}

export function openApiDocs() {
    window.open('/swagger', '_blank');
}

let _startTime = Date.now();
export function updateUptime() {
    // 模拟运行时间（简单）
    const now = Date.now();
    const diffMs = now - _startTime;
    const diffMins = Math.floor(diffMs / 60000);

    const uptimePercent = 100 - (Math.random() * 5);
    const el = document.getElementById('uptime-percent');
    if (el) el.textContent = uptimePercent.toFixed(1) + '%';

    const progressFill = document.querySelector('.progress-fill');
    if (progressFill) progressFill.style.width = uptimePercent + '%';
}

export function showNotification(title, message, type = 'info') {
    const notification = document.getElementById('notification');
    const icon = document.getElementById('notification-icon');
    const notificationTitle = document.getElementById('notification-title');
    const notificationMessage = document.getElementById('notification-message');

    let iconClass;
    switch (type) {
        case 'success': iconClass = 'fa-check-circle'; break;
        case 'error': iconClass = 'fa-exclamation-circle'; break;
        case 'warning': iconClass = 'fa-exclamation-triangle'; break;
        default: iconClass = 'fa-info-circle'; break;
    }

    icon.className = `fas ${iconClass} text-${type === 'success' ? 'green' : type === 'error' ? 'red' : type === 'warning' ? 'yellow' : 'blue'}-400 text-xl mr-3 mt-0.5`;
    notificationTitle.textContent = title;
    notificationMessage.textContent = message;

    notification.classList.remove('translate-y-full');

    setTimeout(() => {
        notification.classList.add('translate-y-full');
    }, 5000);
}