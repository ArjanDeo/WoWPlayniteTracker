local f = CreateFrame("Frame")

function f:OnEvent(event, ...)
    if self[event] then
        self[event](self, event, ...);
    end
end

-- Initialize SavedVariable on load
function f:ADDON_LOADED(event, addonName)
    if addonName == "WoWPlayniteTracker" then
        if PlaytimePerCharacter == nil then
            PlaytimePerCharacter = {}
        end
    end
end

function f:PLAYER_LOGOUT()
    RequestTimePlayed();
end

function f:TIME_PLAYED_MSG(_, totalTimePlayed, timePlayedThisLevel)
    local characterName = UnitName("player")
    local realmName = GetRealmName()
    local parsedName = string.format("%s-%s", characterName, realmName)
    PlaytimePerCharacter[parsedName] = math.floor(totalTimePlayed)
end

f:SetScript("OnEvent", f.OnEvent);
f:RegisterEvent("ADDON_LOADED");
f:RegisterEvent("TIME_PLAYED_MSG");
f:RegisterEvent("PLAYER_LOGOUT");

SLASH_WOWPLAYNITETRACKER1 = "/pnt";
function SlashCmdList.WOWPLAYNITETRACKER(msg)
    local characterName = UnitName("player");
    if PlaytimePerCharacter and PlaytimePerCharacter[characterName] then
        print("Playtime for this character is " .. PlaytimePerCharacter[characterName] .. " hours.");
    else
        print("No playtime data saved for this character yet.");
    end
end
