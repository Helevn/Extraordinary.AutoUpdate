// ¼òµ¥µÄÇ°¶Ë×´Ì¬ÈÝÆ÷
export const state = {
    currentConfig: null,
    fileList: []
};

export function setConfig(cfg) {
    state.currentConfig = cfg;
}

export function setFileList(list) {
    state.fileList = list;
}