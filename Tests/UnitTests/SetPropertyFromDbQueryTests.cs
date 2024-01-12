using Microsoft.VisualStudio.TestTools.UnitTesting;
using Winterdom.BizTalk.PipelineTesting;
using System.IO;
using BizTalkComponents.Utils;
using System;

namespace BizTalkComponents.PipelineComponents.SetPropertyFromDbQuery.Tests.UnitTests
{
    [TestClass]
    public class SetPropertyFromDbQueryTests
    {
        ContextProperty ctxReceivedFileName = new ContextProperty("http://schemas.microsoft.com/BizTalk/2003/file-properties#ReceivedFileName");
        ContextProperty ctxParam1 = new ContextProperty("https://testingcomponents#Param1");
        ContextProperty ctxParam2 = new ContextProperty("https://testingcomponents#Param2");
        ContextProperty ctxParam3 = new ContextProperty("https://testingcomponents#Param2");
        ContextProperty ctxDestinationProperty = new ContextProperty("https://testingcomponents#DestinationProperty");

        [TestMethod]
        public void TestSetString()
        {
            var pipeline = PipelineFactory.CreateEmptyReceivePipeline();
            var component = new SetPropertyFromDBQuery
            {
                Connection = "Server=(LocalDB)\\MSSQLLocalDB;database=master;integrated security=true;",
                DestinationProperty = ctxDestinationProperty.ToPropertyString(),
                NoPromotion = false,
                Query = "select upper({https://testingcomponents#Param1})",
            };
            pipeline.AddComponent(component, PipelineStage.Decode);
            var msg = MessageHelper.CreateFromString("<root></root>");
            string strVal= "soMe ValuE to TesT";
            msg.Context.Write(ctxParam1, strVal);
            var outputs = pipeline.Execute(msg);
            var returnedValue = (string)outputs[0].Context.Read(ctxDestinationProperty);
            Assert.AreEqual(returnedValue, strVal.ToUpper(), false, "The returned value does not match the expected value");
        }

        [TestMethod]
        public void TestSetBool()
        {
            var pipeline = PipelineFactory.CreateEmptyReceivePipeline();
            var component = new SetPropertyFromDBQuery
            {
                Connection = "Server=(LocalDB)\\MSSQLLocalDB;database=master;integrated security=true;",
                DestinationProperty = ctxDestinationProperty.ToPropertyString(),
                NoPromotion = false,
                Query = "select convert(bit,{https://testingcomponents#Param1})",
            };
            pipeline.AddComponent(component, PipelineStage.Decode);
            var msg = MessageHelper.CreateFromString("<root></root>");
            bool testValue = true;
            msg.Context.Write(ctxParam1, testValue);
            var outputs = pipeline.Execute(msg);
            var returnedValue = (bool)outputs[0].Context.Read(ctxDestinationProperty);
            Assert.IsInstanceOfType(returnedValue, testValue.GetType(),"The returned type is not the same.");
            Assert.AreEqual(returnedValue, testValue, "The returned value does not match the expected value");
        }
        [TestMethod]
        public void TestDecimal()
        {
            var pipeline = PipelineFactory.CreateEmptyReceivePipeline();
            var component = new SetPropertyFromDBQuery
            {
                Connection = "Server=(LocalDB)\\MSSQLLocalDB;database=master;integrated security=true;",
                DestinationProperty = ctxDestinationProperty.ToPropertyString(),
                NoPromotion = false,
                Query = "select convert(decimal(18,9),{https://testingcomponents#Param1})",
            };
            pipeline.AddComponent(component, PipelineStage.Decode);
            var msg = MessageHelper.CreateFromString("<root></root>");
            decimal testValue= 823567221.871087612M;
            msg.Context.Write(ctxParam1, testValue);
            var outputs = pipeline.Execute(msg);
            var returnedValue = outputs[0].Context.Read(ctxDestinationProperty);
            Assert.IsInstanceOfType(returnedValue, testValue.GetType(), "The returned type is not the same.");
            Assert.AreEqual((decimal)returnedValue, testValue, "The returned value does not match the expected value");
        }

        [TestMethod]
        public void TestDateTime()
        {
            var pipeline = PipelineFactory.CreateEmptyReceivePipeline();
            var component = new SetPropertyFromDBQuery
            {
                Connection = "Server=(LocalDB)\\MSSQLLocalDB;database=master;integrated security=true;",
                DestinationProperty = ctxDestinationProperty.ToPropertyString(),
                NoPromotion = false,
                Query = "select convert(datetime2,{https://testingcomponents#Param1})",
            };
            pipeline.AddComponent(component, PipelineStage.Decode);
            var msg = MessageHelper.CreateFromString("<root></root>");
            DateTime testValue = DateTime.Now;
            msg.Context.Write(ctxParam1, testValue);
            var outputs = pipeline.Execute(msg);
            var returnedValue = outputs[0].Context.Read(ctxDestinationProperty);
            Assert.IsInstanceOfType(returnedValue, testValue.GetType(), "The returned type is not the same.");
            Assert.AreEqual((DateTime)returnedValue, testValue, "The returned value does not match the expected value");
        }
    }
}
