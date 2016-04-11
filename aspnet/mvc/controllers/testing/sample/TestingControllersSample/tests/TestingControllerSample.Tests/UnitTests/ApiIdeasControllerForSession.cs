using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;
using Moq;
using TestingControllersSample.Api;
using TestingControllersSample.ClientModels;
using TestingControllersSample.Core.Interfaces;
using TestingControllersSample.Core.Model;
using Xunit;
namespace TestingControllerSample.Tests.UnitTests
{
    public class ApiIdeasControllerForSession
    {
        [Fact]
        public void ReturnsHttpNotFoundForInvalidSession()
        {
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            int testSessionId = 123;
            mockRepo.Setup(r => r.GetById(testSessionId)).Returns((BrainstormSession)null);
            var controller = new IdeasController(mockRepo.Object);

            var result = Assert.IsType<HttpNotFoundObjectResult>(controller.ForSession(testSessionId));
        }

        [Fact]
        public void ReturnsIdeasForSession()
        {
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            int testSessionId = 123;
            mockRepo.Setup(r => r.GetById(testSessionId)).Returns(GetTestSession());
            var controller = new IdeasController(mockRepo.Object);

            var result = Assert.IsType<HttpOkObjectResult>(controller.ForSession(testSessionId));
            var returnValue = Assert.IsType<List<IdeaDTO>>(result.Value);
            var idea = returnValue.FirstOrDefault();

            Assert.Equal("One", idea.name);
        }

        private BrainstormSession GetTestSession()
        {
            var session = new BrainstormSession()
            {
                DateCreated = new DateTime(2016, 7, 2),
                Id = 1,
                Name = "Test One"
            };

            var idea = new Idea() {Name = "One"};
            session.AddIdea(idea);
            return session;
        } 
    }
}