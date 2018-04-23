#!/usr/local/bin/node
'use strict';
exports.__esModule = true;
var fs = require("fs");
var defaultVals = [0, 1, 0, 0];
var thresholds = {
    'StayAlive': 0.25,
    'StayAwake': 0.35,
    'StayAwakeCrit': 0.175,
    'Find': 1,
    'Find_Weight': 0.7,
    'Collect': 0.5,
    'Collect_Weight': 0.3
};
1;
var WorldState = /** @class */ (function () {
    function WorldState(vals) {
        this.Damage_Taken = vals[0];
        this.Power_Avail = vals[1];
        this.Inv_Used = vals[2];
        this.Ast_Count = vals[3];
    }
    return WorldState;
}());
var StayAlive = /** @class */ (function () {
    function StayAlive() {
        this.consumptionRange = { Min: 0.01, Max: 0.5 };
    }
    StayAlive.prototype.normilze = function (dmgLevel) {
        if (dmgLevel > thresholds['StayAlive']) {
            return ((dmgLevel - thresholds['StayAlive']) / (1 - thresholds['StayAlive']));
        }
        else {
            return 0;
        }
    };
    return StayAlive;
}());
var StayAwake = /** @class */ (function () {
    function StayAwake() {
        this.consumptionRange = { Min: 0.01, Max: 0.1 };
    }
    StayAwake.prototype.normilze = function (pwrAvailable) {
        if (pwrAvailable < thresholds['StayAwake']) {
            return Math.abs((((pwrAvailable - thresholds['StayAwakeCrit']) / thresholds['StayAwakeCrit']) * 0.5) - 0.5);
        }
        else {
            return 0;
        }
    };
    return StayAwake;
}());
var Collect = /** @class */ (function () {
    function Collect() {
        this.consumptionRange = { Min: 0.0, Max: 0.1 };
    }
    Collect.prototype.normilze = function (invUsed, astLocated) {
        var outputVal = 0.0;
        if (astLocated >= thresholds['Find']) {
            outputVal += 0.7;
        }
        if (invUsed >= thresholds['Collect']) {
            outputVal += (1 - ((invUsed - thresholds['Collect']) / thresholds['Collect'])) * 0.3;
        }
        else {
            outputVal += 0.3;
        }
        return outputVal;
    };
    return Collect;
}());
var AIModule = /** @class */ (function () {
    function AIModule(defaultThresholds, state) {
        this.MyGoal = {};
        this.AvailableGoals = [
            new StayAlive(),
            new StayAwake(),
            new Collect()
        ];
        this.MyState = state;
        this.MyThresholds = defaultThresholds;
        this.MyGoal;
    }
    AIModule.prototype.run = function () {
        var goalRel = [
            { name: 'StayAlive', relivancy: this.AvailableGoals[0].normilze(this.MyState.Damage_Taken), goal: this.AvailableGoals[0] },
            { name: 'StayAwake', relivancy: this.AvailableGoals[1].normilze(this.MyState.Power_Avail), goal: this.AvailableGoals[1] },
            { name: 'CollectResources', relivancy: this.AvailableGoals[2].normilze(this.MyState.Inv_Used, this.MyState.Ast_Count), goal: this.AvailableGoals[2] }
        ];
        var selectedGoal;
        for (var i = 0; i < goalRel.length; i++) {
            if (selectedGoal) {
                selectedGoal = selectedGoal['relivancy'] > goalRel[i]['relivancy'] ? selectedGoal : goalRel[i];
            }
            else {
                selectedGoal = goalRel[i];
            }
        }
        this.MyGoal = selectedGoal;
    };
    return AIModule;
}());
// =====================================
//              Main
// =====================================
var NewWorld = new WorldState(defaultVals);
var AI = new AIModule(thresholds, NewWorld);
var outputData = [];
var outputPath = './data/datafile.json';
for (var i = 0; i < 3; i++) {
    AI.run();
    outputData.push({
        'Current_Goal': AI.MyGoal,
        'States': AI.MyState
    });
}
fs.writeFile(outputPath, JSON.stringify(outputData), function (err) {
    if (err) {
        console.log(err);
    }
    else {
        console.log("Data written successfully to: " + outputPath);
    }
});
