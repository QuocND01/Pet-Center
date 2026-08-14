using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using PetCenterAPI.Controllers;
using PetCenterAPI.Models;
using PetCenterAPI.Service.Interface;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using static PetCenterAPI.DTOs.Requests.Order.OrderRequestDTO;

namespace PetCenterTestProject.ChatTest
{
    public class ChatAPIControllerTest
    {
        private PetCenterContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<PetCenterContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new PetCenterContext(options);
        }

        private ChatAPIController CreateControllerWithUser(PetCenterContext db, Mock<IOrderService> orderServiceMock, Guid userId, string role = "Customer")
        {
            var controller = new ChatAPIController(db, orderServiceMock.Object);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            return controller;
        }

        [Fact]
        public async Task GetMyOrdersWithItems_ReturnsOk_WhenOrdersExist()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var orderServiceMock = new Mock<IOrderService>();
            var userId = Guid.NewGuid();

            var expectedOrders = new List<ReadOrderListWithItemsDTO>
            {
                new ReadOrderListWithItemsDTO
                {
                    OrderId = Guid.NewGuid(),
                    TotalAmount = 250000,
                    OrderDate = DateTime.Now,
                    OrderItems = new List<ReadOrderItemDTO>
                    {
                        new ReadOrderItemDTO
                        {
                            ProductId = Guid.NewGuid(),
                            ProductName = "Thức ăn Hạt Cho Chó Royal Canin",
                            Quantity = 2,
                            UnitPrice = 125000
                        }
                    }
                }
            };

            orderServiceMock.Setup(s => s.GetCustomerOrderHistoryWithItemsAsync(userId))
                .ReturnsAsync(expectedOrders);

            var controller = CreateControllerWithUser(db, orderServiceMock, userId);

            // Act
            var result = await controller.GetMyOrdersWithItems();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualOrders = Assert.IsType<List<ReadOrderListWithItemsDTO>>(okResult.Value);
            Assert.Single(actualOrders);
            Assert.Equal(250000, actualOrders[0].TotalAmount);
        }

        [Fact]
        public async Task GetMyOrdersWithItems_Returns500_WhenServiceThrowsException()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var orderServiceMock = new Mock<IOrderService>();
            var userId = Guid.NewGuid();

            orderServiceMock.Setup(s => s.GetCustomerOrderHistoryWithItemsAsync(userId))
                .ThrowsAsync(new Exception("Database connection failure"));

            var controller = CreateControllerWithUser(db, orderServiceMock, userId);

            // Act
            var result = await controller.GetMyOrdersWithItems();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetChatHistory_ReturnsMessagesBetweenUserAndPartner()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var orderServiceMock = new Mock<IOrderService>();
            var myId = Guid.NewGuid();
            var partnerId = Guid.NewGuid();

            db.ChatMessages.AddRange(
                new ChatMessage { MessageId = Guid.NewGuid(), SenderId = myId, ReceiverId = partnerId, Content = "Chào bạn", CreatedAt = DateTime.UtcNow.AddMinutes(-5) },
                new ChatMessage { MessageId = Guid.NewGuid(), SenderId = partnerId, ReceiverId = myId, Content = "Dạ chào bạn, PetCenter có thể giúp gì ạ?", CreatedAt = DateTime.UtcNow },
                new ChatMessage { MessageId = Guid.NewGuid(), SenderId = myId, ReceiverId = Guid.NewGuid(), Content = "Message to another", CreatedAt = DateTime.UtcNow }
            );
            await db.SaveChangesAsync();

            var controller = CreateControllerWithUser(db, orderServiceMock, myId);

            // Act
            var result = await controller.GetChatHistory(partnerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.Equal(2, System.Linq.Enumerable.Count(list));
        }

        [Fact]
        public async Task GetMyHistory_ReturnsAllUserMessages()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var orderServiceMock = new Mock<IOrderService>();
            var myId = Guid.NewGuid();

            db.ChatMessages.AddRange(
                new ChatMessage { MessageId = Guid.NewGuid(), SenderId = myId, ReceiverId = Guid.NewGuid(), Content = "Hỏi về giá thức ăn chó", CreatedAt = DateTime.UtcNow.AddMinutes(-10) },
                new ChatMessage { MessageId = Guid.NewGuid(), SenderId = Guid.NewGuid(), ReceiverId = myId, Content = "Dạ giá 150k ạ", CreatedAt = DateTime.UtcNow }
            );
            await db.SaveChangesAsync();

            var controller = CreateControllerWithUser(db, orderServiceMock, myId);

            // Act
            var result = await controller.GetMyHistory();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.Equal(2, System.Linq.Enumerable.Count(list));
        }

        [Fact]
        public async Task GetMyCustomers_ReturnsCustomersList_ForStaffRole()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var orderServiceMock = new Mock<IOrderService>();
            var staffId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            db.Customers.Add(new Customer { CustomerId = customerId, FullName = "Nguyễn Văn A", Email = "customer@gmail.com" });
            db.ChatMessages.Add(new ChatMessage { MessageId = Guid.NewGuid(), SenderId = customerId, ReceiverId = staffId, Content = "Tư vấn giúp em", IsRead = false, CreatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();

            var controller = CreateControllerWithUser(db, orderServiceMock, staffId, role: "Sale Staff");

            // Act
            var result = await controller.GetMyCustomers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.Single(list);
        }

        [Fact]
        public async Task MarkAsRead_UpdatesIsReadToTrue()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var orderServiceMock = new Mock<IOrderService>();
            var staffId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var msg = new ChatMessage { MessageId = Guid.NewGuid(), SenderId = customerId, ReceiverId = staffId, Content = "Cần hỗ trợ đổi trả", IsRead = false, CreatedAt = DateTime.UtcNow };
            db.ChatMessages.Add(msg);
            await db.SaveChangesAsync();

            var controller = CreateControllerWithUser(db, orderServiceMock, staffId, role: "Admin");

            // Act
            var result = await controller.MarkAsRead(customerId);

            // Assert
            Assert.IsType<OkResult>(result);
            var updatedMsg = await db.ChatMessages.FindAsync(msg.MessageId);
            Assert.NotNull(updatedMsg);
            Assert.True(updatedMsg.IsRead);
        }
    }
}
