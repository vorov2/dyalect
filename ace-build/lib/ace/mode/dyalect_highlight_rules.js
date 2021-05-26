define(function (require, exports, module) {
    "use strict";

    var oop = require("../lib/oop");
    var DocCommentHighlightRules = require("./doc_comment_highlight_rules").DocCommentHighlightRules;
    var TextHighlightRules = require("./text_highlight_rules").TextHighlightRules;

    var DyalectHighlightRules = function () {
        var keywordMapper = this.createKeywordMapper({
            "variable.language": "this",
            "keyword": "static|func|type|and|or|using|auto|let|var|for|while|do|when|match|is|in|if|else|private|return|yield|continue|break|set|not|throw|try|catch|base|nameof",
            "constant.language": "nil|true|false"
        }, "identifier");

        // regexp must not have capturing parentheses. Use (?:) instead.
        // regexps are ordered -> the first match is used

        this.$rules = {
            "start": [
                {
                    token: "comment",
                    regex: "\\/\\/.*$"
                },
                DocCommentHighlightRules.getStartRule("doc-start"),
                {
                    token: "comment", // multi line comment
                    regex: "\\/\\*",
                    next: "comment"
                }, {
                    token: "string", // character
                    regex: /'(?:.|\\(:?u[\da-fA-F]+|x[\da-fA-F]+|[tbrf'"n]))?'/
                }, {
                    token: "string", start: '"', end: '"|$', next: [
                        { token: "constant.language.escape", regex: /\\(:?u[\da-fA-F]+|x[\da-fA-F]+|[tbrf'"n])/ }
                    ]
                }, {
                    token: "string", start: '\\<\\[', end: '\\]\\>'
                }, {
                    token: "constant.numeric", // hex
                    regex: "0[xX][0-9a-fA-F]+\\b"
                }, {
                    token: "constant.numeric", // float
                    regex: "[+-]?\\d+(?:(?:\\.\\d*)?(?:[eE][+-]?\\d+)?)?\\b"
                }, {
                    token: "constant.language.boolean",
                    regex: "(?:true|false)\\b"
                }, {
                    token: keywordMapper,
                    regex: "[a-zA-Z_$][a-zA-Z0-9_$]*\\b"
                }, {
                    token: "keyword.operator",
                    regex: "!|\\$|%|&&&|\\*|\\-\\-|\\-|\\+\\+|\\+|~~~|\\|\\|\\||==|=|/|\\.|\\:\\:|\\?|\\:|!=|<=|>=|<<<=|>>>=|<>|<|>|!|&&|\\|\\||\\*=|%=|\\+=|\\-=|&&&=|\\^\\^\\^=|\\|\\|\\|=|\\b(?:in|nameof)"
                }, {
                    token: "keyword",
                    regex: "^\\s*#(optimizer|warning)"
                }, {
                    token: "punctuation.operator",
                    regex: "\\?|\\:|\\,|\\;|\\."
                }, {
                    token: "paren.lparen",
                    regex: "[[({]"
                }, {
                    token: "paren.rparen",
                    regex: "[\\])}]"
                }, {
                    token: "text",
                    regex: "\\s+"
                }
            ],
            "comment": [
                {
                    token: "comment", // closing comment
                    regex: "\\*\\/",
                    next: "start"
                }, {
                    defaultToken: "comment"
                }
            ]
        };

        this.normalizeRules();
    };

    oop.inherits(DyalectHighlightRules, TextHighlightRules);

    exports.DyalectHighlightRules = DyalectHighlightRules;
});
