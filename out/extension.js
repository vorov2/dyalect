"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vscode = require("vscode");
const path = require("path");
function activate(context) {
    let out = vscode.window.createOutputChannel("Dyalect");
    const cfg = vscode.workspace.getConfiguration("dyalect");
    const switches = () => {
        var ret = "";
        if (cfg.disableOptimizations) {
            ret += " -nopt";
        }
        if (cfg.disableCompilerWarnings) {
            ret += " -nowarn";
        }
        if (cfg.disableLinkerWarnings) {
            ret += " -nowarnLinker";
        }
        if (cfg.lookupPaths.length != 0) {
            for (var i = 0; i < cfg.lookupPaths.length; i++) {
                ret += ` -path "${cfg.lookupPaths[i]}"`;
            }
        }
        return ret;
    };
    const evalFile = () => {
        const currentFilePath = vscode.window.activeTextEditor.document.fileName;
        out.clear();
        out.show(true);
        const cp = require("child_process");
        const cmd = `dotnet "${cfg.path}" "${currentFilePath}" -time ${switches()}`;
        out.appendLine(cmd);
        cp.exec(cmd, (err, stdout, stderr) => {
            out.appendLine(stdout);
        });
    };
    const compileFile = () => {
        const currentFilePath = vscode.window.activeTextEditor.document.fileName;
        const folder = vscode.workspace.getWorkspaceFolder(vscode.window.activeTextEditor.document.uri);
        var outputPath = path.join(folder.uri.fsPath, "obj");
        out.clear();
        out.show(true);
        const cp = require("child_process");
        const cmd = `dotnet "${cfg.path}" "${currentFilePath}" -compile -out "${outputPath}" ${switches()}`;
        out.appendLine(cmd);
        cp.exec(cmd, (err, stdout, stderr) => {
            out.appendLine(stdout);
        });
    };
    context.subscriptions.push(vscode.commands.registerCommand("dyalect.evalFile", evalFile));
    context.subscriptions.push(vscode.commands.registerCommand("dyalect.compileFile", compileFile));
}
exports.activate = activate;
//# sourceMappingURL=extension.js.map