local cfg_file_list = {}
local all_cfg_load_finish = nil

--- 加载配置表的专用接口
-- @param sfilePath 脚本文件路径
-- @param bAntiCheat 是否反作弊, 密钥随机变化混淆原始数据
function RequireCfg(sfilePath, bAntiCheat)
    table.insert(cfg_file_list, sfilePath)
end

function ReloadCfg()

    if all_cfg_load_finish ~= nil then
        all_cfg_load_finish()
    end
end

function call_all_back()

end