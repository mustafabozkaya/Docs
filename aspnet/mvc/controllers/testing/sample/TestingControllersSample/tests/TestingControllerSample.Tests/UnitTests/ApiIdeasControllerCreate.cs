﻿using System;
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
    public class ApiIdeasControllerCreate
    {
        [Fact]
        public void ReturnsBadRequestGivenInvalidModel()
        {
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            var controller = new IdeasController(mockRepo.Object);
            controller.ModelState.AddModelError("error","some error");

            var result = Assert.IsType<BadRequestObjectResult>(controller.Create(null));
        }

        [Fact]
        public void ReturnsHttpNotFoundForInvalidSession()
        {
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            int testSessionId = 123;
            mockRepo.Setup(r => r.GetById(testSessionId)).Returns((BrainstormSession)null);
            var controller = new IdeasController(mockRepo.Object);

            var result = Assert.IsType<HttpNotFoundObjectResult>(controller.Create(new NewIdeaModel()));
        }


        [Fact]
        public void ReturnsNewlyCreatedIdeaForSession()
        {
            var mockRepo = new Mock<IBrainstormSessionRepository>();
            int testSessionId = 123;
            string testName = "test name";
            string testDescription = "test description";
            var testSession = GetTestSession();
            mockRepo.Setup(r => r.GetById(testSessionId)).Returns(testSession);
            var controller = new IdeasController(mockRepo.Object);

            var newIdea = new NewIdeaModel()
            {
                Description = testDescription,
                Name = testName,
                SessionId = testSessionId
            };
            mockRepo.Setup(r => r.Update(testSession)).Verifiable();

            var result = Assert.IsType<ObjectResult>(controller.Create(newIdea));
            var returnSession = Assert.IsType<BrainstormSession>(result.Value);

            mockRepo.Verify();
            Assert.Equal(2, returnSession.Ideas.Count());
            Assert.Equal(testName, returnSession.Ideas.LastOrDefault().Name);
            Assert.Equal(testDescription, returnSession.Ideas.LastOrDefault().Description);
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