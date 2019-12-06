using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.VM;
using System;
using System.Text;

namespace Neo.Test
{
    [TestClass]
    public class UtScript
    {
        [TestMethod]
        public void Conversion()
        {
            byte[] rawScript;
            using (var builder = new ScriptBuilder())
            {
                builder.Emit(OpCode.PUSH0);
                builder.Emit(OpCode.CALL, new byte[] { 0x00, 0x01 });
                builder.EmitSysCall(123);

                rawScript = builder.ToArray();
            }

            var script = new Script(rawScript);

            byte[] scriptConversion = script;
            CollectionAssert.AreEqual(rawScript, scriptConversion);
        }

        [TestMethod]
        public void Parse()
        {
            Script script;

            using (var builder = new ScriptBuilder())
            {
                builder.Emit(OpCode.PUSH0);
                builder.Emit(OpCode.CALL, new byte[] { 0x00, 0x01 });
                builder.EmitSysCall(123);

                script = new Script(builder.ToArray());
            }

            Assert.AreEqual(9, script.Length);

            var ins = script.GetInstruction(0);

            Assert.AreEqual(OpCode.PUSH0, ins.OpCode);
            Assert.IsTrue(ins.Operand.IsEmpty);
            Assert.AreEqual(1, ins.Size);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { var x = ins.TokenI16; });
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { var x = ins.TokenU32; });

            ins = script.GetInstruction(1);

            Assert.AreEqual(OpCode.CALL, ins.OpCode);
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x01 }, ins.Operand.ToArray());
            Assert.AreEqual(3, ins.Size);
            Assert.AreEqual(256, ins.TokenI16);
            Assert.AreEqual(Encoding.ASCII.GetString(new byte[] { 0x00, 0x01 }), ins.TokenString);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { var x = ins.TokenU32; });

            ins = script.GetInstruction(4);

            Assert.AreEqual(OpCode.SYSCALL, ins.OpCode);
            CollectionAssert.AreEqual(new byte[] { 123, 0x00, 0x00, 0x00 }, ins.Operand.ToArray());
            Assert.AreEqual(5, ins.Size);
            Assert.AreEqual(123, ins.TokenI16);
            Assert.AreEqual(Encoding.ASCII.GetString(new byte[] { 123, 0x00, 0x00, 0x00 }), ins.TokenString);
            Assert.AreEqual(123U, ins.TokenU32);

            ins = script.GetInstruction(100);

            Assert.AreSame(Instruction.RET, ins);
        }
    }
}
