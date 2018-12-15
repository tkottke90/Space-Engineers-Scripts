#!/usr/local/bin/node
'use strict';
Object.defineProperty(exports, "__esModule", { value: true });
const fs = require("fs");
const Goals = require("./lib/Goal.type");
const defaultVals = [0, 1, 0, 0];
const thresholds = Goals.thresholds;
const ModifierValues = [3, 1, 1];
class AIModule {
    constructor(defaultThresholds, state) {
        this.AvailableGoals = [
            new Goals.StayAlive(),
            new Goals.StayAwake(),
            new Goals.Collect()
        ];
        this.MyState = state;
        this.MyThresholds = defaultThresholds;
        this.MyGoal = new Goals.RelevancyObject('StayAlive', 0, new Goals.StayAlive());
    }
    run() {
        let goalRel = [
            new Goals.RelevancyObject('StayAlive', (this.AvailableGoals[0].normilze(this.MyThresholds, this.MyState.Damage_Taken) * ModifierValues[0]), this.AvailableGoals[0]),
            new Goals.RelevancyObject('StayAwake', (this.AvailableGoals[1].normilze(this.MyThresholds, this.MyState.Power_Avail) * ModifierValues[1]), this.AvailableGoals[1]),
            new Goals.RelevancyObject('CollectResources', (this.AvailableGoals[2].normilze(this.MyThresholds, this.MyState.Inv_Used, this.MyState.Ast_Count) * ModifierValues[2]), this.AvailableGoals[2])
        ];
        this.tempList = goalRel;
        let selectedGoal = goalRel[0];
        for (let i = 1; i < goalRel.length; i++) {
            selectedGoal = selectedGoal.relevancy > goalRel[i].relevancy ? selectedGoal : goalRel[i];
        }
        return selectedGoal;
    }
}
// =====================================
//              Main
// =====================================
let NewWorld = new Goals.WorldState(defaultVals);
let AI = new AIModule(thresholds, NewWorld);
let outputData = [];
let outputPath = './data/datafile.json';
let logData = [];
let logPath = './data/logfile.json';
for (let i = 0; i < 500; i++) {
    let prevGoal = AI.MyGoal;
    let newGoal = AI.run();
    if (prevGoal.name != newGoal.name) {
        AI.MyGoal = newGoal;
        logData.push({
            'RunCount': i,
            'Timestamp': new Date(Date.now()).toTimeString(),
            'Event': `New Goal Selected: ${AI.MyGoal.name}`
        });
    }
    outputData.push({
        'RunCount': i,
        'Current_Goal': AI.MyGoal,
        'Relivancy_Scores': {
            'StayAlive': AI.tempList[0].relevancy,
            'StayAwake': AI.tempList[1].relevancy,
            'Collect': AI.tempList[2].relevancy
        },
        'States': AI.MyState.output()
    });
    AI.MyState = AI.MyGoal['goal'].action(AI.MyState);
    if (AI.MyState.Damage_Taken >= 1) {
        logData.push({
            'RunCount': i,
            'Timestamp': new Date(Date.now()).toTimeString(),
            'Event': "Drone Sustained Too Much Damage and Died"
        });
        break;
    }
    else if (AI.MyState.Power_Avail <= 0) {
        logData.push({
            'RunCount': i,
            'Timestamp': new Date(Date.now()).toTimeString(),
            'Event': 'Drone out of energy, simulation failed'
        });
        break;
    }
    else if (AI.MyState.Inv_Used >= 1) {
        logData.push({
            'RunCount': i,
            'Timestamp': new Date(Date.now()).toTimeString(),
            'Event': 'Inventory Full - Returning to Base'
        });
    }
}
fs.writeFile(outputPath, JSON.stringify(outputData), (err) => {
    if (err) {
        console.log(err);
    }
    else {
        console.log(`Data written successfully to: ${outputPath}`);
    }
});
if (logData.length > 0) {
    fs.writeFile(logPath, JSON.stringify(logData), (err) => {
        if (err) {
            console.log(err);
        }
        else {
            console.log(`Data written successfully to: ${logPath}`);
        }
    });
}
else {
    console.log("No Logs Written");
}
