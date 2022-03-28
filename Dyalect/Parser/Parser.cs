
#nullable disable
using System;
using System.Linq;
using System.Collections.Generic;
using Dyalect.Parser.Model;
using DNodeList = System.Collections.Generic.List<Dyalect.Parser.Model.DNode>;


namespace Dyalect.Parser
{
    partial class InternalParser
    {
	public const int _EOF = 0;
	public const int _ucaseToken = 1;
	public const int _lcaseToken = 2;
	public const int _variantToken = 3;
	public const int _directive = 4;
	public const int _intToken = 5;
	public const int _floatToken = 6;
	public const int _stringToken = 7;
	public const int _charToken = 8;
	public const int _implicitToken = 9;
	public const int _verbatimStringToken = 10;
	public const int _autoToken = 11;
	public const int _varToken = 12;
	public const int _letToken = 13;
	public const int _lazyToken = 14;
	public const int _funcToken = 15;
	public const int _returnToken = 16;
	public const int _privateToken = 17;
	public const int _continueToken = 18;
	public const int _breakToken = 19;
	public const int _yieldToken = 20;
	public const int _ifToken = 21;
	public const int _forToken = 22;
	public const int _whileToken = 23;
	public const int _typeToken = 24;
	public const int _inToken = 25;
	public const int _doToken = 26;
	public const int _arrowToken = 27;
	public const int _dotToken = 28;
	public const int _commaToken = 29;
	public const int _semicolonToken = 30;
	public const int _colonToken = 31;
	public const int _equalToken = 32;
	public const int _parenLeftToken = 33;
	public const int _parenRightToken = 34;
	public const int _curlyLeftToken = 35;
	public const int _curlyRightToken = 36;
	public const int _squareLeftToken = 37;
	public const int _squareRightToken = 38;
	public const int _eq_coa = 39;
	public const int _eq_add = 40;
	public const int _eq_sub = 41;
	public const int _eq_mul = 42;
	public const int _eq_div = 43;
	public const int _eq_rem = 44;
	public const int _eq_and = 45;
	public const int _eq_or = 46;
	public const int _eq_xor = 47;
	public const int _eq_lsh = 48;
	public const int _eq_rsh = 49;
	public const int _minus = 50;
	public const int _plus = 51;
	public const int _not = 52;
	public const int _bitnot = 53;
	public const int _coalesce = 54;
	public const int maxT = 101;




        private void Get()
        {
            for (;;)
            {
                t = la;
                la = scanner.Scan();

                if (la.kind <= maxT)
                {
                    ++errDist;
                    break;
                }

                la = t;
            }
        }

	void Separator() {
		Expect(30);
	}

	void StandardOperators() {
		switch (la.kind) {
		case 51: {
			Get();
			break;
		}
		case 50: {
			Get();
			break;
		}
		case 55: {
			Get();
			break;
		}
		case 56: {
			Get();
			break;
		}
		case 57: {
			Get();
			break;
		}
		case 58: {
			Get();
			break;
		}
		case 59: {
			Get();
			break;
		}
		case 52: {
			Get();
			break;
		}
		case 60: {
			Get();
			break;
		}
		case 61: {
			Get();
			break;
		}
		case 62: {
			Get();
			break;
		}
		case 63: {
			Get();
			break;
		}
		case 64: {
			Get();
			break;
		}
		case 65: {
			Get();
			break;
		}
		case 66: {
			Get();
			break;
		}
		case 67: {
			Get();
			break;
		}
		case 68: {
			Get();
			break;
		}
		case 53: {
			Get();
			break;
		}
		default: SynErr(102); break;
		}
	}

	void FunctionName() {
		if (la.kind == 1 || la.kind == 2) {
			Identifier();
		} else if (StartOf(1)) {
			StandardOperators();
		} else SynErr(103);
	}

	void Identifier() {
		if (la.kind == 2) {
			Get();
		} else if (la.kind == 1) {
			Get();
		} else SynErr(104);
	}

	void Qualident(out string name, out Qualident type) {
		type = null; string str1 = null; string str2 = null; string str3 = null; 
		if (la.kind == 1) {
			Get();
			str1 = t.val; 
			if (StartOf(2)) {
				if (la.kind == 28) {
					Get();
					FunctionName();
					str2 = t.val; 
					if (StartOf(2)) {
						if (la.kind == 28) {
							Get();
							Identifier();
							str3 = t.val; 
						} else {
							StandardOperators();
							str3 = t.val; 
						}
					}
				} else {
					StandardOperators();
					str2 = t.val; 
				}
			}
		} else if (la.kind == 2) {
			Get();
			str1 = t.val; 
			if (StartOf(2)) {
				if (la.kind == 28) {
					Get();
					FunctionName();
					str2 = t.val; 
					if (StartOf(2)) {
						if (la.kind == 28) {
							Get();
							Identifier();
							str3 = t.val; 
						} else {
							StandardOperators();
							str3 = t.val; 
						}
					}
				} else {
					StandardOperators();
					str2 = t.val; 
				}
			}
		} else SynErr(105);
		if (str2 is null)
		   name = str1;
		else if (str3 is null)
		{
		   type = new Qualident(str1);
		   name = str2;
		}
		else
		{
		   type = new Qualident(str2, str1);
		   name = str3;
		}
		
	}

	void ImportToken(out string str) {
		str = ""; 
		if (la.kind == 7) {
			Get();
			str = ParseSimpleString(); 
		} else if (la.kind == 1 || la.kind == 2) {
			Identifier();
			str = t.val; 
			while (la.kind == 28) {
				Get();
				Identifier();
				str += string.Concat(".", t.val); 
			}
		} else SynErr(106);
	}

	void Region(out DNode node) {
		string name = null; node = null; 
		Expect(69);
		var ot = t; 
		if (la.kind == 1 || la.kind == 2) {
			Identifier();
			name = t.val; 
		} else if (la.kind == 7) {
			Get();
			name = ParseSimpleString(); 
		} else SynErr(107);
		var block = new DBlock(default);
		var imports = new List<DImport>();
		
		while (StartOf(3)) {
			if (StartOf(4)) {
				Statement(out node);
				block.Nodes.Add(node);
				
			} else {
				Import(out var imp);
				imports.Add(imp); 
				Separator();
			}
		}
		Expect(70);
		var dyc = new DyCodeModel(block, imports.ToArray(), FileName);
		node = new DRegion(name, dyc, ot);
		
	}

	void Statement(out DNode node) {
		node = null; 
		if (StartOf(5)) {
			Binding(out node);
			if (la.kind == 75) {
				Guard(node, out node);
			}
			Separator();
		} else if (la.kind == 81) {
			Rebinding(out node);
			if (la.kind == 75) {
				Guard(node, out node);
			}
			Separator();
		} else if (StartOf(6)) {
			ControlFlow(out node);
			if (la.kind == 75) {
				Guard(node, out node);
			}
			Separator();
		} else if (la.kind == 24) {
			Type(out node);
			Separator();
		} else if (la.kind == 83) {
			Match(out node);
			if (la.kind == 75) {
				Guard(node, out node);
				Separator();
			}
		} else if (IsPrivateScope()) {
			PrivateScope(out node);
		} else if (StartOf(7)) {
			Assignment(out node);
			if (la.kind == 75) {
				Guard(node, out node);
			}
			Separator();
		} else if (la.kind == 21) {
			If(out node);
			if (la.kind == 75) {
				Guard(node, out node);
			}
			Separator();
		} else if (la.kind == 22 || la.kind == 23 || la.kind == 26) {
			Loops(out node);
			if (la.kind == 75) {
				Guard(node, out node);
				Separator();
			}
		} else if (la.kind == 15 || la.kind == 78) {
			Function(out node);
		} else if (la.kind == 4) {
			Directive(out node);
			Separator();
		} else if (la.kind == 69) {
			Region(out node);
		} else SynErr(108);
	}

	void Import(out DImport node) {
		Expect(71);
		var inc = new DImport(t); node = inc; 
		ImportToken(out var str);
		var lastName = str; 
		if (la.kind == 32) {
			Get();
			ImportToken(out str);
			inc.Alias = lastName;
			lastName = str;
			
		}
		while (la.kind == 56) {
			Get();
			ImportToken(out str);
			if (inc.LocalPath != null)
			   inc.LocalPath = string.Concat(inc.LocalPath, "/", lastName);
			else
			   inc.LocalPath = lastName;
			lastName = str;
			
		}
		inc.ModuleName = lastName; 
	}

	void Type(out DNode node) {
		DFunctionDeclaration f = null; 
		Expect(24);
		var typ = new DTypeDeclaration(t);
		node = typ;
		
		Identifier();
		typ.Name = t.val; node = typ; 
		if (la.kind == 32 || la.kind == 33) {
			if (la.kind == 33) {
				f = new DFunctionDeclaration(t) { Name = typ.Name, IsStatic = true, IsConstructor = true, TypeName = new Qualident(typ.Name) }; 
				TypeArguments(f);
				if (la.kind == 35) {
					Block(out var b);
					f.Body = b; 
				}
				typ.Constructors.Add(f); 
			} else {
				Get();
				Constructor(typ);
				while (la.kind == 72) {
					Get();
					Constructor(typ);
				}
			}
		}
	}

	void TypeArguments(DFunctionDeclaration node) {
		Expect(33);
		if (StartOf(8)) {
			TypeArgument(out var arg);
			node.Parameters.Add(arg); 
			while (la.kind == 29) {
				Get();
				TypeArgument(out arg);
				node.Parameters.Add(arg); 
			}
		}
		Expect(34);
	}

	void Block(out DNode node) {
		node = null; 
		Expect(35);
		var block = new DBlock(t); 
		if (StartOf(4)) {
			Statement(out node);
			block.Nodes.Add(node); 
			while (StartOf(4)) {
				Statement(out node);
				block.Nodes.Add(node); 
			}
		}
		node = block; 
		Expect(36);
	}

	void Constructor(DTypeDeclaration typ) {
		var priv = false; 
		if (la.kind == 17) {
			Get();
			priv = true; 
		}
		Identifier();
		var f = new DFunctionDeclaration(t) {
		   Name = t.val,
		   IsPrivate = priv,
		   IsStatic = true, 
		   IsConstructor = true,
		   TypeName = new Qualident(typ.Name) 
		}; 
		typ.Constructors.Add(f);
		
		TypeArguments(f);
		if (la.kind == 35) {
			Block(out var b);
			f.Body = b; 
		}
	}

	void TypeName(out Qualident qual) {
		string val; qual = null; 
		if (la.kind == 1) {
			Get();
			qual = new Qualident(t.val); 
		} else if (la.kind == 2) {
			Get();
			val = t.val; 
			Expect(28);
			Expect(1);
			qual = new Qualident(t.val, val); 
		} else SynErr(109);
	}

	void TypeAnnotation(out TypeAnnotation ta) {
		TypeAnnotation cta = null; 
		TypeName(out var qual);
		if (la.kind == 73) {
			Get();
			TypeAnnotation(out cta);
		}
		ta = new TypeAnnotation(qual, cta); 
	}

	void TypeArgument(out DParameter arg) {
		arg = null; var mut = false; TypeAnnotation ta = null; 
		if (la.kind == 12 || la.kind == 13) {
			if (la.kind == 12) {
				Get();
				mut = true; 
			} else {
				Get();
			}
		}
		if (IsTypeName()) {
			TypeAnnotation(out ta);
		}
		Expect(2);
		arg = new DTypeParameter(t) { Name = t.val, TypeAnnotation = ta, Mutable = mut }; 
		if (la.kind == 32) {
			Get();
			Control(out var cnode);
			arg.DefaultValue = cnode; 
		}
		if (la.kind == 74) {
			Get();
			arg.IsVarArgs = true; 
		}
	}

	void Control(out DNode node) {
		node = null; 
		if (la.kind == 21) {
			If(out node);
		} else if (StartOf(7)) {
			Expr(out node);
		} else if (la.kind == 22 || la.kind == 23 || la.kind == 26) {
			Loops(out node);
		} else if (la.kind == 83) {
			Match(out node);
		} else SynErr(110);
	}

	void Binding(out DNode node) {
		node = null; 
		if (la.kind == 12 || la.kind == 13 || la.kind == 14) {
			if (la.kind == 12) {
				Get();
			} else if (la.kind == 13) {
				Get();
			} else {
				Get();
			}
			var bin = new DBinding(t) { Constant = t.val == "let", Lazy = t.val == "lazy" }; 
			OrPattern(out var pat);
			bin.Pattern = pat; 
			if (la.kind == 32) {
				Get();
				Control(out node);
				bin.Init = node; 
			}
			node = bin; 
		} else if (la.kind == 11) {
			Get();
			var bin = new DBinding(t) { AutoClose = true, Constant = true }; 
			NamePattern(out var pat);
			bin.Pattern = pat; 
			Expect(32);
			Control(out node);
			bin.Init = node; node = bin; 
		} else SynErr(111);
	}

	void Guard(DNode src, out DNode node) {
		node = src; 
		var ot = t; 
		Expect(75);
		Expr(out var cnode);
		var @if = new DIf(ot) { Condition = cnode, True = src }; node = @if; 
	}

	void Rebinding(out DNode node) {
		Expect(81);
		var bin = new DRebinding(t); 
		OrPattern(out var pat);
		bin.Pattern = pat; 
		Expect(32);
		Control(out node);
		bin.Init = node; 
		node = bin; 
	}

	void ControlFlow(out DNode node) {
		node = null; 
		if (la.kind == 19) {
			Break(out node);
		} else if (la.kind == 18) {
			Continue(out node);
		} else if (la.kind == 16) {
			Return(out node);
		} else if (la.kind == 20) {
			Yield(out node);
		} else SynErr(112);
	}

	void Match(out DNode node) {
		node = null; 
		Expect(83);
		var m = new DMatch(t); 
		Control(out node);
		m.Expression = node; 
		Expect(35);
		MatchEntry(out var entry);
		m.Entries.Add(entry); 
		while (la.kind == 29) {
			Get();
			MatchEntry(out entry);
			m.Entries.Add(entry); 
		}
		Expect(36);
		node = m; 
	}

	void PrivateScope(out DNode node) {
		node = null; 
		Expect(17);
		var ot = t; 
		Block(out node);
		node = new DPrivateScope(ot) { Block = (DBlock)node };
		
	}

	void Assignment(out DNode node) {
		StatementExpr(out node);
		if (StartOf(9)) {
			var ass = new DAssignment(t) { Target = node };
			node = ass;
			BinaryOperator? op = null;
			
			switch (la.kind) {
			case 32: {
				Get();
				break;
			}
			case 39: {
				Get();
				op = BinaryOperator.Coalesce; 
				break;
			}
			case 40: {
				Get();
				op = BinaryOperator.Add; 
				break;
			}
			case 41: {
				Get();
				op = BinaryOperator.Sub; 
				break;
			}
			case 42: {
				Get();
				op = BinaryOperator.Mul; 
				break;
			}
			case 43: {
				Get();
				op = BinaryOperator.Div; 
				break;
			}
			case 44: {
				Get();
				op = BinaryOperator.Rem; 
				break;
			}
			case 45: {
				Get();
				op = BinaryOperator.And; 
				break;
			}
			case 46: {
				Get();
				op = BinaryOperator.Or; 
				break;
			}
			case 47: {
				Get();
				op = BinaryOperator.Xor; 
				break;
			}
			case 48: {
				Get();
				op = BinaryOperator.ShiftLeft; 
				break;
			}
			case 49: {
				Get();
				op = BinaryOperator.ShiftRight; 
				break;
			}
			}
			Control(out node);
			ass.Value = node;
			ass.AutoAssign = op;
			node = ass;
			
		}
	}

	void If(out DNode node) {
		node = null; 
		Expect(21);
		var @if = new DIf(t); 
		Control(out node);
		@if.Condition = node; 
		Block(out node);
		@if.True = node; 
		if (la.kind == 87) {
			Get();
			if (la.kind == 35) {
				Block(out node);
				@if.False = node; 
			} else if (la.kind == 21) {
				If(out node);
				@if.False = node; 
			} else SynErr(113);
		}
		node = @if; 
	}

	void Loops(out DNode node) {
		node = null; 
		if (la.kind == 23) {
			While(out node);
		} else if (la.kind == 22) {
			For(out node);
		} else if (la.kind == 26) {
			DoWhile(out node);
			Separator();
		} else SynErr(114);
	}

	void Function(out DNode node) {
		node = null; var st = false; DRecursiveBlock rec = null; 
		if (la.kind == 78) {
			Get();
			st = true; 
		}
		Expect(15);
		FunctionBody(st, out node);
		while (la.kind == 79) {
			if (rec is null)
			{
			   rec = new DRecursiveBlock(t);
			   rec.Functions.Add((DFunctionDeclaration)node);
			}
			
			Get();
			FunctionBody(false, out node);
			rec.Functions.Add((DFunctionDeclaration)node); 
		}
		node = rec ?? node; 
	}

	void Directive(out DNode node) {
		Expect(4);
		var pp = new DDirective(t) { Key = t.val.Substring(1) }; node = pp; 
		while (StartOf(10)) {
			if (la.AfterEol) return; 
			switch (la.kind) {
			case 7: {
				Get();
				pp.Attributes.Add(ParseSimpleString()); 
				break;
			}
			case 5: {
				Get();
				pp.Attributes.Add(ParseInteger()); 
				break;
			}
			case 6: {
				Get();
				pp.Attributes.Add(ParseFloat()); 
				break;
			}
			case 8: {
				Get();
				pp.Attributes.Add(ParseChar()); 
				break;
			}
			case 76: {
				Get();
				pp.Attributes.Add(true); 
				break;
			}
			case 77: {
				Get();
				pp.Attributes.Add(false); 
				break;
			}
			case 1: case 2: {
				Identifier();
				pp.Attributes.Add(t.val); 
				break;
			}
			}
		}
	}

	void Expr(out DNode node) {
		node = null; 
		if (IsFunction()) {
			Lambda(out node);
		} else if (la.kind == 27) {
			NullaryLambda(out node);
		} else if (StartOf(11)) {
			Coalesce(out node);
			if (la.kind == 90) {
				Ternary(node, out node);
			}
		} else if (la.kind == 91) {
			TryCatch(out node);
		} else if (la.kind == 89) {
			ThrowExpr(out node);
		} else if (la.kind == 35) {
			Block(out node);
		} else SynErr(115);
	}

	void StatementExpr(out DNode node) {
		node = null; 
		if (IsFunction()) {
			Lambda(out node);
		} else if (la.kind == 27) {
			NullaryLambda(out node);
		} else if (StartOf(11)) {
			Coalesce(out node);
			if (la.kind == 90) {
				Ternary(node, out node);
			}
		} else if (la.kind == 91) {
			TryCatch(out node);
		} else if (la.kind == 89) {
			Throw(out node);
		} else if (la.kind == 35) {
			Block(out node);
		} else SynErr(116);
	}

	void FunctionBody(bool st, out DNode node) {
		node = null; var get = false; var set = false; 
		if (la.kind == 80 || la.kind == 81) {
			if (la.kind == 80) {
				Get();
				get = true; 
			} else {
				Get();
				set = true; 
			}
		}
		var f = new DFunctionDeclaration(t) { IsStatic = st, Getter = get, Setter = set };
		functions.Push(f);
		
		Qualident(out var name, out Qualident type);
		f.Name = name;
		f.TypeName = type;
		
		if (la.kind == 37) {
			Get();
			f.IsIndexer = true;
			if (f.TypeName is not null && f.TypeName.Parent is not null)
			   AddError(ParserError.InvalidTypeName, t.GetLocation());
			else if (f.TypeName is not null)
			   f.TypeName = new Qualident(f.Name, f.TypeName.Local);
			else
			   f.TypeName = new Qualident(f.Name);
			
			if (la.kind == 1 || la.kind == 2) {
				FunctionParameters(f);
			}
			f.Name = set ? "__op_set" : "__op_get"; 
			Expect(38);
		} else if (la.kind == 33) {
			Get();
			if (la.kind == 1 || la.kind == 2) {
				FunctionParameters(f);
			}
			Expect(34);
		} else if (la.kind == 82) {
			Get();
			if (f.TypeName is not null && f.TypeName.Parent is not null)
			   AddError(ParserError.InvalidTypeName, t.GetLocation());
			else if (f.TypeName is not null)
			   f.TypeName = new Qualident(f.Name, f.TypeName.Local);
			else
			   f.TypeName = new Qualident(f.Name);
			f.Name = null;
			
			TypeName(out var qual);
			f.TargetTypeName = qual; 
		} else SynErr(117);
		if (la.kind == 35) {
			Block(out node);
		} else if (la.kind == 27) {
			Get();
			FunctionStatement(out node);
		} else SynErr(118);
		f.Body = node;
		node = f;
		functions.Pop();
		
	}

	void FunctionParameters(DFunctionDeclaration node) {
		FunctionParameter(out var arg);
		node.Parameters.Add(arg); 
		while (la.kind == 29) {
			Get();
			FunctionParameter(out arg);
			node.Parameters.Add(arg); 
		}
	}

	void FunctionStatement(out DNode node) {
		node = null; 
		switch (la.kind) {
		case 11: case 12: case 13: case 14: {
			Binding(out node);
			if (la.kind == 75) {
				Guard(node, out node);
			}
			Separator();
			break;
		}
		case 81: {
			Rebinding(out node);
			if (la.kind == 75) {
				Guard(node, out node);
			}
			Separator();
			break;
		}
		case 16: case 18: case 19: case 20: {
			ControlFlow(out node);
			if (la.kind == 75) {
				Guard(node, out node);
			}
			Separator();
			break;
		}
		case 83: {
			Match(out node);
			if (la.kind == 75) {
				Guard(node, out node);
				Separator();
			}
			break;
		}
		case 1: case 2: case 3: case 5: case 6: case 7: case 8: case 10: case 17: case 27: case 33: case 35: case 37: case 50: case 51: case 52: case 53: case 76: case 77: case 84: case 86: case 89: case 91: case 96: case 97: case 99: case 100: {
			Assignment(out node);
			if (la.kind == 75) {
				Guard(node, out node);
			}
			Separator();
			break;
		}
		case 21: {
			If(out node);
			if (la.kind == 75) {
				Guard(node, out node);
				Separator();
			}
			break;
		}
		case 22: case 23: case 26: {
			Loops(out node);
			if (la.kind == 75) {
				Guard(node, out node);
				Separator();
			}
			break;
		}
		default: SynErr(119); break;
		}
	}

	void FunctionParameter(out DParameter arg) {
		TypeAnnotation ta = null; 
		if (IsTypeName()) {
			TypeAnnotation(out ta);
		}
		Expect(2);
		arg = new DParameter(t) { Name = t.val, TypeAnnotation = ta }; 
		if (la.kind == 32) {
			Get();
			Control(out var cnode);
			arg.DefaultValue = cnode; 
		}
		if (la.kind == 74) {
			Get();
			arg.IsVarArgs = true; 
		}
	}

	void OrPattern(out DPattern node) {
		node = null; 
		AndPattern(out node);
		while (la.kind == 72) {
			var por = new DOrPattern(t) { Left = node }; 
			Get();
			AndPattern(out node);
			por.Right = node; node = por; 
		}
	}

	void NamePattern(out DPattern node) {
		node = null; Token ot = null; string mod = null; string typ = null; string nam = null;
		Identifier();
		nam = t.val; ot = t; 
		if (la.kind == 28) {
			Get();
			Identifier();
			typ = nam; nam = t.val; 
			if (la.kind == 28) {
				Get();
				Identifier();
				mod = typ; typ = nam; nam = t.val; 
			}
		}
		if (la.kind == 33) {
			var ctor = new DCtorPattern(t);
			if (mod is null && typ is null)
			   ctor.Constructor = nam;
			else if (mod is null) {
			   ctor.TypeName = new Qualident(typ);
			   ctor.Constructor = nam;
			}
			else {
			   ctor.TypeName = new Qualident(typ, mod);
			   ctor.Constructor = nam;
			}
			
			CtorPatternArguments(ctor.Arguments);
			node = ctor; 
		}
		if (node is null)
		{
		   if (mod is not null && typ is not null && nam is not null)
		       AddError(ParserError.InvalidPattern, t.GetLocation());
		   else if (mod is not null && typ is not null && nam is null)
		   {
		       node = new DTypeTestPattern(ot) {
		           TypeName = typ is null ? new Qualident(nam) : new Qualident(typ, mod) 
		       };
		   }
		   else if (nam is not null)
		   {
		       if (nam == "_")
		           node = new DWildcardPattern(ot);
		       else if (char.IsUpper(nam[0]))
		           node = new DTypeTestPattern(ot) { TypeName = new Qualident(nam) };
		       else
		           node = new DNamePattern(ot) { Name = nam };
		   }
		}
		
	}

	void MatchEntry(out DMatchEntry me) {
		me = new DMatchEntry(t); 
		OrPattern(out var p);
		me.Pattern = p; 
		if (la.kind == 75) {
			Get();
			Control(out var node);
			me.Guard = node; 
		}
		Expect(27);
		Assignment(out var exp);
		me.Expression = exp; 
	}

	void AndPattern(out DPattern node) {
		node = null; 
		RangePattern(out node);
		while (la.kind == 79) {
			var pa = new DAndPattern(t) { Left = node }; 
			Get();
			RangePattern(out node);
			pa.Right = node; node = pa; 
		}
	}

	void RangePattern(out DPattern node) {
		node = null; 
		Pattern(out node);
		if (la.kind == 84) {
			var r = new DRangePattern(t) { From = node }; 
			Get();
			Pattern(out node);
			r.To = node; node = r; 
		}
	}

	void Pattern(out DPattern node) {
		node = null; 
		if (la.kind == 1 || la.kind == 2) {
			NamePattern(out node);
			if (la.kind == 1 || la.kind == 2) {
				AsPattern(node, out node);
			}
		} else if (la.kind == 3) {
			VariantPattern(out node);
			if (la.kind == 1 || la.kind == 2) {
				AsPattern(node, out node);
			}
		} else if (la.kind == 5) {
			IntegerPattern(out node);
			if (la.kind == 1 || la.kind == 2) {
				AsPattern(node, out node);
			}
		} else if (la.kind == 6) {
			FloatPattern(out node);
			if (la.kind == 1 || la.kind == 2) {
				AsPattern(node, out node);
			}
		} else if (la.kind == 8) {
			CharPattern(out node);
			if (la.kind == 1 || la.kind == 2) {
				AsPattern(node, out node);
			}
		} else if (la.kind == 7) {
			StringPattern(out node);
			if (la.kind == 1 || la.kind == 2) {
				AsPattern(node, out node);
			}
		} else if (la.kind == 76 || la.kind == 77) {
			BooleanPattern(out node);
			if (la.kind == 1 || la.kind == 2) {
				AsPattern(node, out node);
			}
		} else if (la.kind == 86) {
			NilPattern(out node);
			if (la.kind == 1 || la.kind == 2) {
				AsPattern(node, out node);
			}
		} else if (IsTuple(allowFields: true)) {
			TuplePattern(out node);
			if (la.kind == 1 || la.kind == 2) {
				AsPattern(node, out node);
			}
		} else if (la.kind == 33) {
			GroupPattern(out node);
			if (la.kind == 1 || la.kind == 2) {
				AsPattern(node, out node);
			}
		} else if (la.kind == 37) {
			ArrayPattern(out node);
			if (la.kind == 1 || la.kind == 2) {
				AsPattern(node, out node);
			}
		} else if (la.kind == 28) {
			MethodCheckPattern(out node);
			if (la.kind == 1 || la.kind == 2) {
				AsPattern(node, out node);
			}
		} else if (la.kind == 85) {
			NotPattern(out node);
		} else if (StartOf(12)) {
			ComparisonPattern(out node);
		} else SynErr(120);
	}

	void AsPattern(DPattern target, out DPattern node) {
		node = null; 
		if (la.AfterEol) { node = target; return; }
		var asp = new DAsPattern(t) { Pattern = target };
		
		Identifier();
		asp.Name = t.val; node = asp; 
	}

	void VariantPattern(out DPattern node) {
		Expect(3);
		var ctor = new DCtorPattern(t) { Constructor = t.val.Substring(1) }; node = ctor; 
		if (la.kind == 33) {
			CtorPatternArguments(ctor.Arguments);
		}
	}

	void IntegerPattern(out DPattern node) {
		Expect(5);
		node = new DIntegerPattern(t) { Value = ParseInteger() }; 
	}

	void FloatPattern(out DPattern node) {
		Expect(6);
		node = new DFloatPattern(t) { Value = ParseFloat() }; 
	}

	void CharPattern(out DPattern node) {
		Expect(8);
		node = new DCharPattern(t) { Value = ParseChar() }; 
	}

	void StringPattern(out DPattern node) {
		Expect(7);
		node = new DStringPattern(t) { Value = ParseString() }; 
	}

	void BooleanPattern(out DPattern node) {
		if (la.kind == 76) {
			Get();
		} else if (la.kind == 77) {
			Get();
		} else SynErr(121);
		node = new DBooleanPattern(t) { Value = t.val == "true" }; 
	}

	void NilPattern(out DPattern node) {
		Expect(86);
		node = new DNilPattern(t); 
	}

	void TuplePattern(out DPattern node) {
		node = null; 
		Expect(33);
		var tup = new DTuplePattern(t); 
		if (IsLabelPattern()) {
			LabelPattern(out node);
		} else if (StartOf(13)) {
			OrPattern(out node);
		} else SynErr(122);
		tup.Elements.Add(node); 
		while (la.kind == 29) {
			Get();
			if (StartOf(13)) {
				if (IsLabelPattern()) {
					LabelPattern(out node);
				} else {
					OrPattern(out node);
				}
				tup.Elements.Add(node); 
			}
		}
		node = tup; 
		Expect(34);
	}

	void GroupPattern(out DPattern node) {
		Expect(33);
		OrPattern(out node);
		Expect(34);
	}

	void ArrayPattern(out DPattern node) {
		node = null; 
		Expect(37);
		var tup = new DArrayPattern(t); 
		RangePattern(out node);
		tup.Elements.Add(node); 
		while (la.kind == 29) {
			Get();
			RangePattern(out node);
			tup.Elements.Add(node); 
		}
		node = tup; 
		Expect(38);
	}

	void MethodCheckPattern(out DPattern node) {
		var name = ""; 
		Expect(28);
		if (la.kind == 1 || la.kind == 2) {
			Identifier();
			name = t.val; 
		} else if (la.kind == 33) {
			Get();
			switch (la.kind) {
			case 50: {
				Get();
				break;
			}
			case 51: {
				Get();
				break;
			}
			case 55: {
				Get();
				break;
			}
			case 56: {
				Get();
				break;
			}
			case 60: {
				Get();
				break;
			}
			case 61: {
				Get();
				break;
			}
			case 66: {
				Get();
				break;
			}
			case 63: {
				Get();
				break;
			}
			case 62: {
				Get();
				break;
			}
			case 64: {
				Get();
				break;
			}
			case 65: {
				Get();
				break;
			}
			case 52: {
				Get();
				break;
			}
			case 57: {
				Get();
				break;
			}
			case 67: {
				Get();
				break;
			}
			case 68: {
				Get();
				break;
			}
			case 53: {
				Get();
				break;
			}
			case 58: {
				Get();
				break;
			}
			case 59: {
				Get();
				break;
			}
			default: SynErr(123); break;
			}
			name = t.val; 
			Expect(34);
		} else SynErr(124);
		node = new DMethodCheckPattern(t) { Name = name }; 
	}

	void NotPattern(out DPattern node) {
		Expect(85);
		var np = new DNotPattern(t); 
		Pattern(out node);
		np.Pattern = node; node = np; 
	}

	void ComparisonPattern(out DPattern node) {
		node = null; BinaryOperator op = default; Token ot = default; 
		if (la.kind == 62) {
			Get();
			op = BinaryOperator.Gt; ot = t; 
		} else if (la.kind == 63) {
			Get();
			op = BinaryOperator.Lt; ot = t; 
		} else if (la.kind == 64) {
			Get();
			op = BinaryOperator.GtEq; ot = t; 
		} else if (la.kind == 65) {
			Get();
			op = BinaryOperator.LtEq; ot = t; 
		} else SynErr(125);
		switch (la.kind) {
		case 5: {
			IntegerPattern(out node);
			break;
		}
		case 6: {
			FloatPattern(out node);
			break;
		}
		case 8: {
			CharPattern(out node);
			break;
		}
		case 7: {
			StringPattern(out node);
			break;
		}
		case 76: case 77: {
			BooleanPattern(out node);
			break;
		}
		case 86: {
			NilPattern(out node);
			break;
		}
		default: SynErr(126); break;
		}
		node = new DComparisonPattern(ot) { Operator = op, Pattern = node }; 
	}

	void LabelPattern(out DPattern node) {
		node = null; 
		Identifier();
		var la = new DLabelPattern(t) { Label = t.val }; 
		Expect(31);
		Pattern(out var pat);
		la.Pattern = pat; node = la; 
	}

	void CtorPatternArguments(DNodeList arguments) {
		DPattern node = null; 
		Expect(33);
		if (StartOf(13)) {
			if (IsLabelPattern()) {
				LabelPattern(out node);
			} else {
				OrPattern(out node);
			}
			arguments.Add(node); 
		}
		while (la.kind == 29) {
			Get();
			if (IsLabelPattern()) {
				LabelPattern(out node);
			} else if (StartOf(13)) {
				OrPattern(out node);
			} else SynErr(127);
			arguments.Add(node); 
		}
		Expect(34);
	}

	void While(out DNode node) {
		node = null; 
		Expect(23);
		var @while = new DWhile(t); 
		Control(out node);
		@while.Condition = node; 
		Block(out node);
		@while.Body = node;
		node = @while;
		
	}

	void For(out DNode node) {
		node = null; 
		Expect(22);
		var @for = new DFor(t); 
		OrPattern(out var pattern);
		@for.Pattern = pattern; 
		Expect(25);
		Control(out node);
		@for.Target = node; 
		if (la.kind == 75) {
			Get();
			Control(out node);
			@for.Guard = node; 
		}
		Block(out node);
		@for.Body = node;
		node = @for;
		
	}

	void DoWhile(out DNode node) {
		node = null; 
		var @while = new DWhile(t) { DoWhile = true }; 
		Expect(26);
		Block(out node);
		@while.Body = node; 
		Expect(23);
		Control(out node);
		@while.Condition = node; node = @while; 
	}

	void Break(out DNode node) {
		Expect(19);
		var br = new DBreak(t); node = br; 
		if (StartOf(14)) {
			if (la.AfterEol) return; 
			Control(out var exp);
			br.Expression = exp; 
		}
	}

	void Continue(out DNode node) {
		Expect(18);
		node = new DContinue(t); 
	}

	void Return(out DNode node) {
		Expect(16);
		var br = new DReturn(t); node = br;
		if (la.AfterEol) return;
		
		if (StartOf(14)) {
			Control(out var exp);
			br.Expression = exp; 
		}
	}

	void Yield(out DNode node) {
		Expect(20);
		var ot = t;
		node = null;
		if (functions.Count > 0)
		   functions.Peek().IsIterator = true;
		
		if (la.kind == 19) {
			Get();
			node = new DYieldBreak(ot); 
		} else if (la.kind == 88) {
			Get();
			var yield = new DYieldMany(t);
			node = yield;
			
			Control(out var exp);
			yield.Expression = exp; 
		} else if (StartOf(14)) {
			var yield = new DYield(t);
			node = yield;
			
			Control(out var exp);
			yield.Expression = exp; 
		} else SynErr(128);
	}

	void Lambda(out DNode node) {
		var f = new DFunctionDeclaration(t);
		node = f;
		
		if (la.kind == 1 || la.kind == 2) {
			FunctionParameter(out var a);
			f.Parameters.Add(a); 
		} else if (la.kind == 33) {
			Get();
			if (la.kind == 1 || la.kind == 2) {
				FunctionParameters(f);
			}
			Expect(34);
		} else SynErr(129);
		functions.Push(f); 
		Expect(27);
		Expr(out var exp);
		f.Body = exp; functions.Pop(); 
	}

	void NullaryLambda(out DNode node) {
		var f = new DFunctionDeclaration(t);
		node = f;
		
		functions.Push(f); 
		Expect(27);
		Expr(out var exp);
		f.Body = exp; functions.Pop(); 
	}

	void Coalesce(out DNode node) {
		Or(out node);
		while (la.kind == 54) {
			Get();
			var ot = t; 
			Or(out DNode exp);
			node = new DBinaryOperation(node, exp, BinaryOperator.Coalesce, ot); 
		}
	}

	void Ternary(DNode parent, out DNode node) {
		node = null; 
		Expect(90);
		var @if = new DIf(t) { Condition = parent }; 
		Expr(out node);
		@if.True = node; 
		Expect(31);
		Expr(out node);
		@if.False = node; node = @if; 
	}

	void TryCatch(out DNode node) {
		node =  null; 
		Expect(91);
		var tc = new DTryCatch(t); 
		Block(out node);
		tc.Expression = node; 
		Expect(92);
		if (la.kind == 35) {
			var m = new DMatch(t); tc.Catch = m; 
			Get();
			MatchEntry(out var entry);
			m.Entries.Add(entry); 
			while (la.kind == 29) {
				Get();
				MatchEntry(out entry);
				m.Entries.Add(entry); 
			}
			Expect(36);
		} else if (la.kind == 1 || la.kind == 2) {
			Identifier();
			tc.BindVariable = new DName(t) { Value = t.val }; 
			Block(out node);
			tc.Catch = node; 
		} else SynErr(130);
		node = tc; 
	}

	void Throw(out DNode node) {
		node = null; 
		Expect(89);
		var th = new DThrow(t);
		node = th;
		if (la.AfterEol) return;
		
		if (StartOf(14)) {
			Control(out var cexp);
			th.Expression = cexp; 
		}
	}

	void ThrowExpr(out DNode node) {
		node = null; 
		Expect(89);
		var th = new DThrow(t);
		node = th;
		
		Control(out var cexp);
		th.Expression = cexp; 
	}

	void Or(out DNode node) {
		And(out node);
		while (la.kind == 93) {
			Get();
			var ot = t; 
			And(out DNode exp);
			node = new DBinaryOperation(node, exp, BinaryOperator.Or, ot); 
		}
	}

	void And(out DNode node) {
		Is(out node);
		while (la.kind == 94) {
			Get();
			var ot = t; 
			Is(out DNode exp);
			node = new DBinaryOperation(node, exp, BinaryOperator.And, ot); 
		}
	}

	void Is(out DNode node) {
		In(out node);
		while (la.kind == 95) {
			Get();
			var ot = t; 
			OrPattern(out var pat);
			node = new DBinaryOperation(node, pat, BinaryOperator.Is, ot); 
		}
	}

	void In(out DNode node) {
		Range(out node);
		while (la.kind == 25) {
			Get();
			var ot = t; 
			Range(out var pat);
			node = new DBinaryOperation(node, pat, BinaryOperator.In, ot); 
		}
	}

	void Range(out DNode node) {
		node = null; DNode snode = null; DNode cnode = null; bool exclu = false; 
		if (la.kind == 96) {
			Get();
			FunctionApplication(out snode);
			if (la.kind == 84) {
				Get();
			} else if (la.kind == 97) {
				Get();
				exclu = true; 
			} else SynErr(131);
			var range = new DRange(t) { From = node, Exclusive = exclu, Step = snode }; node = range; 
			if (StartOf(15)) {
				FunctionApplication(out cnode);
				range.To = cnode; 
			}
		} else if (la.kind == 84 || la.kind == 97) {
			if (la.kind == 84) {
				Get();
			} else {
				Get();
				exclu = true; 
			}
			var range = new DRange(t) { From = node, Exclusive = exclu }; node = range; 
			if (StartOf(15)) {
				FunctionApplication(out cnode);
				range.To = cnode; 
			}
		} else if (StartOf(15)) {
			Eq(out node);
			if (la.kind == 96) {
				Get();
				FunctionApplication(out snode);
			}
			if (la.kind == 84 || la.kind == 97) {
				if (la.kind == 84) {
					Get();
				} else {
					Get();
					exclu = true; 
				}
				var range = new DRange(t) { From = node, Exclusive = exclu, Step = snode }; node = range; 
				if (StartOf(15)) {
					FunctionApplication(out cnode);
					range.To = cnode; 
				}
			}
		} else SynErr(132);
	}

	void FunctionApplication(out DNode node) {
		node = null; 
		Unary(out node);
		while (la.kind == 98) {
			var app = new DApplication(node, t); 
			Get();
			Unary(out node);
			app.Arguments.Add(node); node = app; 
		}
	}

	void Eq(out DNode node) {
		Shift(out node);
		while (StartOf(16)) {
			var op = default(BinaryOperator);
			var ot = default(Token);
			
			switch (la.kind) {
			case 62: {
				Get();
				ot = t; op = BinaryOperator.Gt; 
				break;
			}
			case 63: {
				Get();
				ot = t; op = BinaryOperator.Lt; 
				break;
			}
			case 64: {
				Get();
				ot = t; op = BinaryOperator.GtEq; 
				break;
			}
			case 65: {
				Get();
				ot = t; op = BinaryOperator.LtEq; 
				break;
			}
			case 60: {
				Get();
				ot = t; op = BinaryOperator.Eq; 
				break;
			}
			case 61: {
				Get();
				ot = t; op = BinaryOperator.NotEq; 
				break;
			}
			}
			Shift(out var exp);
			node = new DBinaryOperation(node, exp, op, ot); 
		}
	}

	void Shift(out DNode node) {
		BitOr(out node);
		while (la.kind == 67 || la.kind == 68) {
			var op = default(BinaryOperator);
			var ot = default(Token);
			
			if (la.kind == 67) {
				Get();
				ot = t; op = BinaryOperator.ShiftLeft; 
			} else {
				Get();
				ot = t; op = BinaryOperator.ShiftRight; 
			}
			BitOr(out var exp);
			node = new DBinaryOperation(node, exp, op, ot); 
		}
	}

	void BitOr(out DNode node) {
		Xor(out node);
		while (la.kind == 58) {
			Get();
			var ot = t; 
			Xor(out var exp);
			node = new DBinaryOperation(node, exp, BinaryOperator.BitwiseOr, ot); 
		}
	}

	void Xor(out DNode node) {
		BitAnd(out node);
		while (la.kind == 66) {
			DNode exp = null; 
			Get();
			var ot = t; 
			BitAnd(out exp);
			node = new DBinaryOperation(node, exp, BinaryOperator.Xor, ot); 
		}
	}

	void BitAnd(out DNode node) {
		Add(out node);
		while (la.kind == 59) {
			Get();
			var ot = t; 
			Add(out var exp);
			node = new DBinaryOperation(node, exp, BinaryOperator.BitwiseAnd, ot); 
		}
	}

	void Add(out DNode node) {
		Mul(out node);
		while (la.kind == 50 || la.kind == 51) {
			var op = default(BinaryOperator);
			var ot = default(Token);
			
			if (la.kind == 51) {
				Get();
				ot = t; op = BinaryOperator.Add; 
			} else {
				Get();
				ot = t; op = BinaryOperator.Sub; 
			}
			Mul(out var exp);
			node = new DBinaryOperation(node, exp, op, ot); 
		}
	}

	void Mul(out DNode node) {
		Cast(out node);
		while (la.kind == 55 || la.kind == 56 || la.kind == 57) {
			var op = default(BinaryOperator);
			var ot = default(Token);
			
			if (la.kind == 55) {
				Get();
				ot = t; op = BinaryOperator.Mul; 
			} else if (la.kind == 56) {
				Get();
				ot = t; op = BinaryOperator.Div; 
			} else {
				Get();
				ot = t; op = BinaryOperator.Rem; 
			}
			Cast(out var exp);
			node = new DBinaryOperation(node, exp, op, ot); 
		}
	}

	void Cast(out DNode node) {
		node = null; 
		FunctionApplication(out node);
		while (la.kind == 82) {
			var @as = new DAs(t) { Expression = node }; string s1, s2 = null; 
			Get();
			Identifier();
			s1 = t.val; 
			if (la.kind == 28) {
				Get();
				Identifier();
				s2 = t.val; 
			}
			@as.Expression = node;
			@as.TypeName = s2 is null ? new Qualident(s1) : new Qualident(s2, s1);
			node = @as;
			
		}
	}

	void Unary(out DNode node) {
		node = null;
		var op = default(UnaryOperator);
		var ot = default(Token);
		
		if (la.kind == 52) {
			Get();
			ot = t; op = UnaryOperator.Not; 
			Index(out node);
			node = new DUnaryOperation(node, op, ot); 
		} else if (la.kind == 50) {
			Get();
			ot = t; op = UnaryOperator.Neg; 
			Index(out node);
			node = new DUnaryOperation(node, op, ot); 
		} else if (la.kind == 51) {
			Get();
			ot = t; op = UnaryOperator.Plus; 
			Index(out node);
			node = new DUnaryOperation(node, op, ot); 
		} else if (la.kind == 53) {
			Get();
			ot = t; op = UnaryOperator.BitwiseNot; 
			Index(out node);
			node = new DUnaryOperation(node, op, ot); 
		} else if (StartOf(17)) {
			Index(out node);
		} else SynErr(133);
	}

	void Index(out DNode node) {
		Literal(out node);
		while (la.kind == 28 || la.kind == 33 || la.kind == 37) {
			if (la.kind == 28) {
				Get();
				var ot = t; 
				Identifier();
				var fld = new DAccess(ot) { Target = node };
				fld.Name = t.val;
				node = fld;
				
			} else if (la.kind == 37) {
				if (la.AfterEol) return; 
				Get();
				var idx = new DIndexer(t) { Target = node }; 
				Control(out node);
				idx.Index = node;
				node = idx;
				
				Expect(38);
			} else {
				if (la.AfterEol) return; 
				Get();
				var app = new DApplication(node, t); 
				if (StartOf(18)) {
					ApplicationArguments(app.Arguments);
				}
				node = app; 
				Expect(34);
			}
		}
	}

	void Literal(out DNode node) {
		node = null; 
		if (la.kind == 1 || la.kind == 2 || la.kind == 17) {
			Name(out node);
		} else if (la.kind == 5) {
			Integer(out node);
		} else if (la.kind == 6) {
			Float(out node);
		} else if (la.kind == 7 || la.kind == 10) {
			String(out node);
		} else if (la.kind == 8) {
			Char(out node);
		} else if (la.kind == 76 || la.kind == 77) {
			Bool(out node);
		} else if (la.kind == 86) {
			Nil(out node);
		} else if (IsTuple()) {
			Tuple(out node);
		} else if (la.kind == 33) {
			Group(out node);
		} else if (la.kind == 100) {
			Base(out node);
		} else if (la.kind == 37) {
			Array(out node);
		} else if (la.kind == 99) {
			Iterator(out node);
		} else if (la.kind == 3) {
			Variant(out node);
		} else SynErr(134);
	}

	void ApplicationArguments(DNodeList arguments) {
		var node = default(DNode); 
		if (IsLabel()) {
			Label(out node);
		} else if (StartOf(14)) {
			Control(out node);
		} else SynErr(135);
		arguments.Add(node); 
		while (la.kind == 29) {
			Get();
			if (IsLabel()) {
				Label(out node);
			} else if (StartOf(14)) {
				Control(out node);
			} else SynErr(136);
			arguments.Add(node); 
		}
	}

	void Label(out DNode node) {
		bool mut = false; node = null; var name = ""; var fromStr = false; 
		if (la.kind == 12 || la.kind == 13) {
			if (la.kind == 12) {
				Get();
				mut = true; 
			} else {
				Get();
			}
		}
		if (la.kind == 1 || la.kind == 2) {
			Identifier();
			name = t.val; 
		} else if (la.kind == 7) {
			Get();
			name = ParseSimpleString(); fromStr = true; 
		} else SynErr(137);
		Expect(31);
		var ot = t; 
		if (IsFunction()) {
			Lambda(out node);
		} else if (StartOf(11)) {
			Is(out node);
		} else SynErr(138);
		node = new DLabelLiteral(ot) { Mutable = mut, Label = name, FromString = fromStr, Expression = node }; 
	}

	void Name(out DNode node) {
		if (la.kind == 1 || la.kind == 2) {
			Identifier();
		} else if (la.kind == 17) {
			Get();
		} else SynErr(139);
		node = new DName(t) { Value = t.val }; 
	}

	void Integer(out DNode node) {
		Expect(5);
		node = new DIntegerLiteral(t) { Value = ParseInteger() }; 
	}

	void Float(out DNode node) {
		Expect(6);
		node = new DFloatLiteral(t) { Value = ParseFloat() }; 
	}

	void String(out DNode node) {
		node = null; 
		if (la.kind == 7) {
			Get();
			node = ParseString(); 
		} else if (la.kind == 10) {
			Get();
			node = ParseVerbatimString(); 
		} else SynErr(140);
	}

	void Char(out DNode node) {
		Expect(8);
		node = new DCharLiteral(t) { Value = ParseChar() }; 
	}

	void Bool(out DNode node) {
		if (la.kind == 76) {
			Get();
		} else if (la.kind == 77) {
			Get();
		} else SynErr(141);
		node = new DBooleanLiteral(t) { Value = t.val == "true" }; 
	}

	void Nil(out DNode node) {
		Expect(86);
		node = new DNilLiteral(t); 
	}

	void Tuple(out DNode node) {
		node = null; 
		Expect(33);
		var tup = new DTupleLiteral(t); 
		if (IsLabel()) {
			Label(out node);
		} else if (StartOf(14)) {
			Control(out node);
		} else SynErr(142);
		tup.Elements.Add(node); 
		while (la.kind == 29) {
			Get();
			if (StartOf(18)) {
				if (IsLabel()) {
					Label(out node);
				} else {
					Control(out node);
				}
				tup.Elements.Add(node); 
			}
		}
		node = tup; 
		Expect(34);
	}

	void Group(out DNode node) {
		node = null; 
		Expect(33);
		Control(out node);
		Expect(34);
	}

	void Base(out DNode node) {
		Expect(100);
		node = new DBase(t); 
	}

	void Array(out DNode node) {
		node = null; 
		Expect(37);
		var arr = new DArrayLiteral(t); 
		if (StartOf(14)) {
			Control(out node);
			arr.Elements.Add(node); 
			while (la.kind == 29) {
				Get();
				Control(out node);
				arr.Elements.Add(node); 
			}
		}
		node = arr; 
		Expect(38);
	}

	void Iterator(out DNode node) {
		node = null; 
		Expect(99);
		Expect(35);
		var it = new DIteratorLiteral(t);
		it.YieldBlock = new DYieldBlock(t);
		
		if (StartOf(14)) {
			Control(out node);
			it.YieldBlock.Elements.Add(node); 
		}
		while (la.kind == 29) {
			Get();
			Control(out node);
			it.YieldBlock.Elements.Add(node); 
		}
		node = it; 
		Expect(36);
	}

	void Variant(out DNode node) {
		string name; node = null; 
		Expect(3);
		name = t.val.Substring(1); 
		var vr = new DVariant(name, t);
		node = vr;
		if (la.AfterEol || la.kind != _parenLeftToken) return;
		
		Expect(33);
		if (StartOf(18)) {
			ApplicationArguments(vr.Arguments);
		}
		Expect(34);
	}

	void DyalectItem() {
		if (StartOf(4)) {
			Statement(out var node);
			Root.Nodes.Add(node);
			
		} else if (la.kind == 71) {
			Import(out var node);
			Imports.Add(node); 
			Separator();
		} else SynErr(143);
	}

	void Dyalect() {
		DyalectItem();
		while (StartOf(3)) {
			DyalectItem();
		}
	}



        public void Parse()
        {
            la = new Token();
            la.val = "";
            Get();
		Dyalect();
		Expect(0);

        }

        static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_x,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_T,_T, _x,_x,_x,_x, _x,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_T, _x,_x,_x,_x, _T,_T,_T,_x, _x,_T,_x,_T, _T,_x,_T,_x, _x,_T,_x,_T, _x,_x,_x,_x, _T,_T,_x,_T, _T,_x,_x},
		{_x,_T,_T,_T, _T,_T,_T,_T, _T,_x,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_x,_T,_T, _x,_x,_x,_x, _x,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _T,_T,_T,_x, _x,_T,_x,_T, _T,_x,_T,_x, _x,_T,_x,_T, _x,_x,_x,_x, _T,_T,_x,_T, _T,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_T,_T,_T, _x,_T,_T,_T, _T,_x,_T,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _T,_x,_T,_x, _x,_T,_x,_T, _x,_x,_x,_x, _T,_T,_x,_T, _T,_x,_x},
		{_x,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_T, _T,_T,_T,_T, _T,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_T,_T,_x, _x,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_T,_T,_T, _x,_T,_T,_T, _T,_x,_T,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _T,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_T, _T,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_T,_T,_T, _x,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_T,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_T,_T,_T, _x,_T,_T,_T, _T,_x,_T,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_T,_T,_T, _x,_x,_T,_T, _x,_x,_x,_x, _x,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_T, _T,_x,_T,_x, _x,_T,_x,_T, _x,_x,_x,_x, _T,_T,_x,_T, _T,_x,_x},
		{_x,_T,_T,_T, _x,_T,_T,_T, _T,_x,_T,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x},
		{_x,_T,_T,_T, _x,_T,_T,_T, _T,_x,_T,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_x,_x},
		{_x,_T,_T,_T, _x,_T,_T,_T, _T,_x,_T,_x, _T,_T,_x,_x, _x,_T,_x,_x, _x,_T,_T,_T, _x,_x,_T,_T, _x,_x,_x,_x, _x,_T,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_T, _T,_x,_T,_x, _x,_T,_x,_T, _x,_x,_x,_x, _T,_T,_x,_T, _T,_x,_x}

        };

        private void SynErr(int line, int col, int n)
        {
            string s;

            switch (n)
            {
			case 0: s = "EOF expected"; break;
			case 1: s = "ucaseToken expected"; break;
			case 2: s = "lcaseToken expected"; break;
			case 3: s = "variantToken expected"; break;
			case 4: s = "directive expected"; break;
			case 5: s = "intToken expected"; break;
			case 6: s = "floatToken expected"; break;
			case 7: s = "stringToken expected"; break;
			case 8: s = "charToken expected"; break;
			case 9: s = "implicitToken expected"; break;
			case 10: s = "verbatimStringToken expected"; break;
			case 11: s = "autoToken expected"; break;
			case 12: s = "varToken expected"; break;
			case 13: s = "letToken expected"; break;
			case 14: s = "lazyToken expected"; break;
			case 15: s = "funcToken expected"; break;
			case 16: s = "returnToken expected"; break;
			case 17: s = "privateToken expected"; break;
			case 18: s = "continueToken expected"; break;
			case 19: s = "breakToken expected"; break;
			case 20: s = "yieldToken expected"; break;
			case 21: s = "ifToken expected"; break;
			case 22: s = "forToken expected"; break;
			case 23: s = "whileToken expected"; break;
			case 24: s = "typeToken expected"; break;
			case 25: s = "inToken expected"; break;
			case 26: s = "doToken expected"; break;
			case 27: s = "arrowToken expected"; break;
			case 28: s = "dotToken expected"; break;
			case 29: s = "commaToken expected"; break;
			case 30: s = "semicolonToken expected"; break;
			case 31: s = "colonToken expected"; break;
			case 32: s = "equalToken expected"; break;
			case 33: s = "parenLeftToken expected"; break;
			case 34: s = "parenRightToken expected"; break;
			case 35: s = "curlyLeftToken expected"; break;
			case 36: s = "curlyRightToken expected"; break;
			case 37: s = "squareLeftToken expected"; break;
			case 38: s = "squareRightToken expected"; break;
			case 39: s = "eq_coa expected"; break;
			case 40: s = "eq_add expected"; break;
			case 41: s = "eq_sub expected"; break;
			case 42: s = "eq_mul expected"; break;
			case 43: s = "eq_div expected"; break;
			case 44: s = "eq_rem expected"; break;
			case 45: s = "eq_and expected"; break;
			case 46: s = "eq_or expected"; break;
			case 47: s = "eq_xor expected"; break;
			case 48: s = "eq_lsh expected"; break;
			case 49: s = "eq_rsh expected"; break;
			case 50: s = "minus expected"; break;
			case 51: s = "plus expected"; break;
			case 52: s = "not expected"; break;
			case 53: s = "bitnot expected"; break;
			case 54: s = "coalesce expected"; break;
			case 55: s = "\"*\" expected"; break;
			case 56: s = "\"/\" expected"; break;
			case 57: s = "\"%\" expected"; break;
			case 58: s = "\"|||\" expected"; break;
			case 59: s = "\"&&&\" expected"; break;
			case 60: s = "\"==\" expected"; break;
			case 61: s = "\"!=\" expected"; break;
			case 62: s = "\">\" expected"; break;
			case 63: s = "\"<\" expected"; break;
			case 64: s = "\">=\" expected"; break;
			case 65: s = "\"<=\" expected"; break;
			case 66: s = "\"^^^\" expected"; break;
			case 67: s = "\"<<<\" expected"; break;
			case 68: s = "\">>>\" expected"; break;
			case 69: s = "\"#region\" expected"; break;
			case 70: s = "\"#endregion\" expected"; break;
			case 71: s = "\"import\" expected"; break;
			case 72: s = "\"or\" expected"; break;
			case 73: s = "\"|\" expected"; break;
			case 74: s = "\"...\" expected"; break;
			case 75: s = "\"when\" expected"; break;
			case 76: s = "\"true\" expected"; break;
			case 77: s = "\"false\" expected"; break;
			case 78: s = "\"static\" expected"; break;
			case 79: s = "\"and\" expected"; break;
			case 80: s = "\"get\" expected"; break;
			case 81: s = "\"set\" expected"; break;
			case 82: s = "\"as\" expected"; break;
			case 83: s = "\"match\" expected"; break;
			case 84: s = "\"..\" expected"; break;
			case 85: s = "\"not\" expected"; break;
			case 86: s = "\"nil\" expected"; break;
			case 87: s = "\"else\" expected"; break;
			case 88: s = "\"many\" expected"; break;
			case 89: s = "\"throw\" expected"; break;
			case 90: s = "\"?\" expected"; break;
			case 91: s = "\"try\" expected"; break;
			case 92: s = "\"catch\" expected"; break;
			case 93: s = "\"||\" expected"; break;
			case 94: s = "\"&&\" expected"; break;
			case 95: s = "\"is\" expected"; break;
			case 96: s = "\"^\" expected"; break;
			case 97: s = "\"..<\" expected"; break;
			case 98: s = "\"\\\\\" expected"; break;
			case 99: s = "\"yields\" expected"; break;
			case 100: s = "\"base\" expected"; break;
			case 101: s = "??? expected"; break;
			case 102: s = "invalid StandardOperators"; break;
			case 103: s = "invalid FunctionName"; break;
			case 104: s = "invalid Identifier"; break;
			case 105: s = "invalid Qualident"; break;
			case 106: s = "invalid ImportToken"; break;
			case 107: s = "invalid Region"; break;
			case 108: s = "invalid Statement"; break;
			case 109: s = "invalid TypeName"; break;
			case 110: s = "invalid Control"; break;
			case 111: s = "invalid Binding"; break;
			case 112: s = "invalid ControlFlow"; break;
			case 113: s = "invalid If"; break;
			case 114: s = "invalid Loops"; break;
			case 115: s = "invalid Expr"; break;
			case 116: s = "invalid StatementExpr"; break;
			case 117: s = "invalid FunctionBody"; break;
			case 118: s = "invalid FunctionBody"; break;
			case 119: s = "invalid FunctionStatement"; break;
			case 120: s = "invalid Pattern"; break;
			case 121: s = "invalid BooleanPattern"; break;
			case 122: s = "invalid TuplePattern"; break;
			case 123: s = "invalid MethodCheckPattern"; break;
			case 124: s = "invalid MethodCheckPattern"; break;
			case 125: s = "invalid ComparisonPattern"; break;
			case 126: s = "invalid ComparisonPattern"; break;
			case 127: s = "invalid CtorPatternArguments"; break;
			case 128: s = "invalid Yield"; break;
			case 129: s = "invalid Lambda"; break;
			case 130: s = "invalid TryCatch"; break;
			case 131: s = "invalid Range"; break;
			case 132: s = "invalid Range"; break;
			case 133: s = "invalid Unary"; break;
			case 134: s = "invalid Literal"; break;
			case 135: s = "invalid ApplicationArguments"; break;
			case 136: s = "invalid ApplicationArguments"; break;
			case 137: s = "invalid Label"; break;
			case 138: s = "invalid Label"; break;
			case 139: s = "invalid Name"; break;
			case 140: s = "invalid String"; break;
			case 141: s = "invalid Bool"; break;
			case 142: s = "invalid Tuple"; break;
			case 143: s = "invalid DyalectItem"; break;

                default:
                    s = "unknown " + n;
                    break;
            }

            AddError(s, line, col);
        }
    }
}
